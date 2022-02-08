const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp()
const path = require('path');
const os = require('os');
const fs = require('fs');
const jimp = require('jimp');
const { on } = require('events');

exports.processImage = functions.storage.object().onFinalize(async (object) => {
    const fileBucket = object.bucket;
    const filePath = object.name;

    const filePathInfo = path.parse(filePath);

    const contentType = object.contentType;
    const processed = object.metadata.processed;
    if (!contentType.startsWith('image/')) {
        return functions.logger.log('Error: Saved file is not an image.');
    }
    if (processed !== 'false') {
        return functions.logger.log('Error: Already processed.');
    }
    const bucket = admin.storage().bucket(fileBucket);
    const tempSourceFilePath = path.join(os.tmpdir(), filePathInfo.base);
    const tempDestinationFilePath = path.join(os.tmpdir(), filePathInfo.name + '_processed.png');
    const newMetadata = Object.assign(object.metadata);
    newMetadata.processed = true;
    await bucket.file(filePath).download({destination: tempSourceFilePath});
    functions.logger.log('Image downloaded locally to', tempSourceFilePath);
    
    var jimpImage = await jimp.read(tempSourceFilePath);
    if(jimpImage.bitmap.width > jimpImage.bitmap.height){
      x = jimpImage.bitmap.width / 2 - jimpImage.bitmap.height / 2;
      w = jimpImage.bitmap.height;
      jimpImage.crop(x, 0, w, jimpImage.bitmap.height).resize(512, 512).circle().write(tempDestinationFilePath);
    }
    else{
      y = jimpImage.bitmap.height / 2 - jimpImage.bitmap.width / 2;
      h = jimpImage.bitmap.width;
      jimpImage.crop(0, y, jimpImage.bitmap.width, h).resize(512, 512).circle().write(tempDestinationFilePath);
    }

    const db = admin.database();
    const ref = db.ref('images/' + filePathInfo.dir);
    const imageRef = ref.push();
    
    await bucket.upload(tempDestinationFilePath, {
      destination: filePathInfo.dir + '/' + imageRef.key + '.png',
      metadata: {
        metadata: {
          processed: true,
        },
      },
    });

    imageRef.set(filePathInfo.dir + '/' + imageRef.key + '.png');
    await bucket.file(filePath).delete();

    functions.logger.log("Success");
    return fs.unlinkSync(tempSourceFilePath);
});