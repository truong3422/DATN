using Firebase.Auth;
using Firebase.Storage;

namespace PoolComVnWebAPI.Common
{
    public class FirebaseCloud
    {
        public static string ApiKey = "AIzaSyDbVNJE6bbQdXlcr3TZqxkZh3xqi5CqKIc";
        public static string Bucket = "poolcomvn-82664.appspot.com";
        public static string AuthEmail = "vuducduy@gmail.com";
        public static string AuthPassword = "123456";
        public async Task<string> UploadFromFirebase(MemoryStream stream, string filename)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var cancellation = new CancellationTokenSource();
            var task = new FirebaseStorage(
         Bucket,
         new FirebaseStorageOptions
         {
             AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
             ThrowOnCancel = true
         }
     ).Child("News")
      .Child(filename)
      .PutAsync(stream, cancellation.Token);
            try
            {
                string link = await task;
                return link;

            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception was thrown : {0}", ex);
                return null;
            }
        }

        public async Task DeleteFromFirebase(string filename)
        {
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                var cancellation = new CancellationTokenSource();
                var storage = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                );
                var oldAvatarPath = $"User/{filename}";
                await storage.Child(oldAvatarPath).DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during deletion: {0}", ex);
            }
        }

    }
}
