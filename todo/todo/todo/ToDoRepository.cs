using SolidDotNet;
using System.Diagnostics;
using todo.Data;

namespace todo
{
    /// <summary>
    /// Handles actions related to the To Do list we're going to render on page. Intended to be somewhat analgous to a repository. 
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
        /// Checks for the existence of the "to do" folder and creates it if it does not exist
        /// </summary>
        /// <param name="toDoFolderName"></param>
        /// <returns></returns>
        public async Task InitAsync(string toDoFolderName)
        {
            if (_solidClient is not null)
            {
                _folderName = toDoFolderName;

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
        /// Returns a list of To Do items stored at the pod
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<ToDo>> GetAll()
        {
            var result = await _solidClient.GetContentsOfContainer(_folderName);

            if (!result.Any(r => r.AbsoluteUri.Contains(TODO_FILENAME)))
            {
                DebugOut("We need to create the to do document");
                string rdfDocument = string.Empty;
                await _solidClient.CreateRdfDocumentAsync(_folderName, TODO_FILENAME, rdfDocument);
            }
            else
            {
                // we need to read the to do list document
                DebugOut("We need to read the to do document");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a To Do item to the pod
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Add(ToDo item)
        {
            throw new NotImplementedException();
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

            return _uri + TODO_FILENAME;
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
