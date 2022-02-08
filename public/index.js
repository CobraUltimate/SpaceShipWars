var suscribedToAuthentication = false;

const save = function(path, dataJson){
    window.database.ref(path).set(JSON.parse(dataJson));
}

const remove = function(path){
    window.database.ref(path).remove();
}

const init = function(){
    window.database = firebase.database();
    var playersRef = window.database.ref('players');
    playersRef.on('child_added', onPlayerAdded);
    playersRef.on('child_changed', onPlayerChanged);
    playersRef.on('child_removed', onPlayerRemoved);
}

const onPlayerAdded = function(snapshot){
    unityInstance.SendMessage('MultiplayerEngine', 'OnPlayerAdded', JSON.stringify(snapshot.val()));
}

const onPlayerChanged = function(snapshot){
    unityInstance.SendMessage('MultiplayerEngine', 'OnPlayerChanged', JSON.stringify(snapshot.val()));
}

const onPlayerRemoved = function(snapshot){
    unityInstance.SendMessage('MultiplayerEngine', 'OnPlayerRemoved', JSON.stringify(snapshot.val()));
}

const signInWithGoogle = function(){
    var provider = new firebase.auth.GoogleAuthProvider();
    firebase.auth().signInWithPopup(provider);
}

const suscribeToAuthentication = function(){
    firebase.auth().onAuthStateChanged(listenToSignIn);
    suscribedToAuthentication = true;
}

const isSuscribedToAuthentication = function(){ 
    return suscribedToAuthentication;
}

const listenToSignIn = function(user){
    userJson = user ? JSON.stringify(user) : ""
    unityInstance.SendMessage('ScenesEngine', 'OnUserAuthenticated', userJson);
}

const onUserImagesCollectionChanged = function(snapshot){
    if(!snapshot.val()) return;
    var storage = firebase.storage().ref();
    var imageRef = storage.child(snapshot.val());
    imageRef.getDownloadURL().then(url => {
        fetch(url).then(image => {
            image.arrayBuffer().then(buffer => {
                imageInfo = {};
                imageInfo.StoragePath = snapshot.val();
                imageInfo.ImageBase64 =  arrayBufferToBase64(buffer);
                imageInfo.RealtimeDatabaseId = snapshot.key;
                unityInstance.SendMessage('SpritesEngine', 'OnUserImagesCollectionChanged', JSON.stringify(imageInfo));
            });
        });
    });
}

const getUserImage = function(playerId, cloudStorageImagePath){
    if(!cloudStorageImagePath || cloudStorageImagePath === '') return;
    var storage = firebase.storage().ref();
    var imageRef = storage.child(cloudStorageImagePath);
    imageRef.getDownloadURL().then(url => {
        fetch(url).then(image => {
            image.arrayBuffer().then(buffer => {
                playerImageInfo = {};
                playerImageInfo.PlayerId = playerId;
                playerImageInfo.ImageBase64 =  arrayBufferToBase64(buffer);
                unityInstance.SendMessage('MultiplayerEngine', 'OnPlayerImageDownloaded', JSON.stringify(playerImageInfo));
            });
        });
    });
}

const onUserImagesCollectionItemDeleted = function(snapshot){
    unityInstance.SendMessage('SpritesEngine', 'OnUserImagesCollectionItemDeleted', JSON.stringify(snapshot.val()));
}

const listenToUserImages = function(){
    var userImagesRef = firebase.database().ref('images/' + firebase.auth().currentUser.uid);
    userImagesRef.on('child_added', onUserImagesCollectionChanged);
    userImagesRef.on('child_changed', onUserImagesCollectionChanged);
    userImagesRef.on('child_removed', onUserImagesCollectionItemDeleted);
}

const logOut = function(){
    firebase.auth().signOut();
}

const getUserId = function(){
    return firebase.auth().currentUser.uid;
}

const fireUpdateSprite = function(){
    const spriteInput = document.getElementById("spriteInput");
    spriteInput.click();
}

const onSpriteSelected = function(e){
    storage = firebase.storage().ref();
    file = e.target.files[0];
    user = firebase.auth().currentUser.uid;
    const metadata = {
        customMetadata: {
            'processed': false,
        },
    };
    storage.child(user + '/' + file.name).put(file, metadata);
}

const onStart = function(){
    document.getElementById("spriteInput").onchange = onSpriteSelected;
}

function arrayBufferToBase64( buffer ) {
    var binary = '';
    var bytes = new Uint8Array( buffer );
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode( bytes[ i ] );
    }
    return window.btoa( binary );
}