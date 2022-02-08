mergeInto(LibraryManager.library, {

  Save: function (path, dataJson) {
    save(Pointer_stringify(path), Pointer_stringify(dataJson));
  },
  
  Delete: function (path) {
    remove(Pointer_stringify(path));
  },

  Init: function () {
    init();
  },

  SuscribeToAuthentication: function(){
    suscribeToAuthentication();
  },

  IsSuscribedToAuthentication: function(){
    return isSuscribedToAuthentication();
  },

  SignInWithGoogle: function(){
    signInWithGoogle();
  },

  LogOut: function(){
    logOut();
  },

  GetUserId: function(){
    userId = getUserId()
    var bufferSize = lengthBytesUTF8(userId) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(userId, buffer, bufferSize);
    return buffer;
  },

  FireUpdateSprite: function(){
    fireUpdateSprite();
  },

  GetUserImage: function(playerId, cloudStorageImagePath){
    getUserImage(Pointer_stringify(playerId), Pointer_stringify(cloudStorageImagePath));
  },

  ListenToUserImages: function(){
    listenToUserImages();
  }

});