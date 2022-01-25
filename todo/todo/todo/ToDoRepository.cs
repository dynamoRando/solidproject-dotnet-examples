using SolidDotNet;
using System.Diagnostics;
using todo.Data;

namespace todo
{
    /// <summary>
    /// A backing data store for our To Do items. Intended to be somewhat analgous to a repository. 
    /// </summary>
    /// <remarks>Internally, this communicates to our Solid Pod via a <see cref="SolidClient"/>.</remarks>
    public class ToDoRepository
    {
        #region Private Fields
        private string _uri;
        private const string TODO_FILENAME = "index.ttl";
        private ToDoDocumentManager _docManager;
        private SolidClient _solidClient;
        private bool _useDebug = true;
        private string _folderName;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public ToDoRepository()
        {
            _docManager = new ToDoDocumentManager();
            _solidClient = new SolidClient();
        }
        #endregion

        #region Public Methods
       
        /// <summary>
        /// Checks for the existence of the "to do" folder at our Solid Pod and creates it if it does not exist
        /// </summary>
        /// <param name="toDoFolderName"></param>
        /// <returns></returns>
        public async Task InitAsync(string toDoFolderName)
        {
            if (_solidClient is not null)
            {
                _uri = _solidClient.IdentityProviderUrl;
                _folderName = toDoFolderName;
                _docManager.ConfigureBaseUri(GetDocumentUri());

                var folder = await _solidClient.GetOrCreateFolderAsync(toDoFolderName);
                if (folder is not null)
                {
                    DebugOut($"{folder.OriginalString} exists!");
                }
                else
                {
                    DebugOut("Unable to find or create folder");
                }
            }
        }

        /// <summary>
        /// Returns a list of To Do items stored at the Solid Pod
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<ToDo>> GetAll()
        {
            var hasFile = await _solidClient.ContainerHasFile(_folderName, TODO_FILENAME);

            if (!hasFile)
            {
                DebugOut("We need to create the to do document");

                // leverage the ToDoDocumentManager here to create RDF text
                _docManager.SetupPrefixes();
                string rdfDocument = _docManager.ToString();
                await _solidClient.CreateRdfDocumentAsync(_folderName, TODO_FILENAME, rdfDocument);
            }
            else
            {
                DebugOut("We need to read the to do document");

                // grab the RDF raw text and pass to ToDoDocumentManager to handle
                var rdfText = await _solidClient.GetRdfDocument(_folderName, TODO_FILENAME);
                var items = _docManager.ParseDocument(rdfText);
                return items;
            }

            return null;
        }

        /// <summary>
        /// Adds a To Do item to the pod
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task AddAsync(ToDo item)
        {
            _docManager.AddToDo(item);
            var rdfText = _docManager.ToString();
            await _solidClient.UpdateRdfDocumentAsync(_folderName, TODO_FILENAME,rdfText);
        }

        /// <summary>
        /// Sets the internal SolidClient for this repository
        /// </summary>
        /// <param name="client">The Solid Client</param>
        public void SetSolidClient(SolidClient client)
        {
            _solidClient = client;
        }

        /// <summary>
        /// Sets the full path of the folder at the pod
        /// </summary>
        /// <param name="uri">The uri of the folder to store this</param>
        public void SetUriFolder(string uri)
        {
            _uri = uri;
        }

        /// <summary>
        /// Returns the uri for the to do list
        /// </summary>
        /// <returns></returns>
        public string GetDocumentUri()
        {
            if (!_uri.EndsWith("/"))
            {
                _uri = _uri + "/";
            }

            var result = _uri + _folderName + "/" + TODO_FILENAME;
            return result;
        }
        #endregion

        #region Private Methods
        private void DebugOut(string item)
        {
            if (_useDebug)
            {
                Console.WriteLine(item);
                Debug.WriteLine(item);
            }
        }
        #endregion

    }
}
