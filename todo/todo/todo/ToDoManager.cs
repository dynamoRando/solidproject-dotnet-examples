using SolidDotNet;
using todo.Data;

namespace todo
{
    /// <summary>
    /// Handles actions related to the To Do list we're going to render on page
    /// </summary>
    public class ToDoManager
    {
        #region Private Fields
        private string _uri;
        private const string TODO_FILENAME = "index.ttl";
        private ToDoDocumentManager _docManager;
        private SolidClient _solidClient;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public ToDoManager()
        {
            _docManager = new ToDoDocumentManager();
        }
        #endregion

        #region Public Methods
        public void Add(ToDo item)
        {
            throw new NotImplementedException();
        }

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

        #endregion

    }
}
