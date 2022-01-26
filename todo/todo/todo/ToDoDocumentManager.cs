using System.Diagnostics;
using todo.Data;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;

namespace todo
{
    /// <summary>
    /// Handles the underlying RDF document stored at our pod
    /// </summary>
    public class ToDoDocumentManager
    {
        #region Private Fields
        private bool _useDebug = true;

        private string _baseUri = string.Empty;

        // a graph represents an RDF document
        private Graph _graph;

        // to write out to Turtle format
        private TurtleWriter _turtleWriter;

        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public ToDoDocumentManager()
        {
            _graph = new Graph();
            _turtleWriter = new TurtleWriter();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Parses the supplied rdf text and returns a list of To Do items in that document
        /// </summary>
        /// <param name="rdfText">The RDF document from our Solid Pod</param>
        /// <returns>A List of To Do Items</returns>
        public List<ToDo> ParseDocument(string rdfText)
        {
            var parser = new TurtleParser();
            _graph.BaseUri = new Uri(_baseUri);
            var reader = new StringReader(rdfText);
            parser.Load(_graph, reader);

            return GetToDosFromGraph();
        }

        /// <summary>
        /// Sets the base Uri for the underlying RDF document
        /// </summary>
        /// <param name="uri"></param>
        public void ConfigureBaseUri(string uri)
        {
            _baseUri = uri;
            _graph.BaseUri = new Uri(uri);
            SetupPrefixes();
        }

        /// <summary>
        ///  Updates the specified to do item in the RDF document with the provided text
        /// </summary>
        /// <param name="id"></param>
        /// <param name="todoText"></param>
        public void UpdateToDo(int id, string todoText)
        {
            var itemToUpdate = GetToDosFromGraph().Where(todo => todo.Id == id).FirstOrDefault();

            if (itemToUpdate is not null)
            {
                try
                {
                    itemToUpdate.Text = todoText;
                    RemoveToDo(id);
                    AddToDo(itemToUpdate);
                }
                catch (Exception ex)
                {
                    DebugOut(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Removes the to do item from the RDF document
        /// </summary>
        /// <param name="id">The id of the to do item</param>
        public void RemoveToDo(int id)
        {
            try
            {
                // the to do item id to remove
                var subjectToDelete = _graph.CreateUriNode(new Uri(_baseUri + "#" + id));
                var triples = _graph.GetTriplesWithSubject(subjectToDelete);
                _graph.Retract(triples);
            }
            catch (Exception ex)
            {
                DebugOut(ex.ToString());
            }
        }

        /// <summary>
        /// Adds a to do item as a RDF Triple using Triple Predicates
        /// </summary>
        /// <param name="item">The to do item to add</param>
        public void AddToDo(ToDo item)
        {
            // https://www.w3.org/TR/turtle/#predicate-lists
            // this will produce an example of predicate lists
            // where we have 1 subject, but multiple predicates

            var subjectNode = _graph.CreateUriNode(new Uri(_baseUri + "#" + item.Id));

            var predicateType = _graph.CreateUriNode("rdf:");
            var objectType = _graph.CreateUriNode("type:");

            var predicateCreated = _graph.CreateUriNode("created:");
            var objectCreatedType = _graph.CreateLiteralNode(DateTime.Now.ToString());

            var predicateText = _graph.CreateUriNode("text:");
            var objectText = _graph.CreateLiteralNode(item.Text);

            _graph.Assert(subjectNode, predicateType, objectType);
            _graph.Assert(subjectNode, predicateCreated, objectCreatedType);
            _graph.Assert(subjectNode, predicateText, objectText);
        }

        /// <summary>
        /// Configures the backing RDF document with the needed prefixes
        /// </summary>
        public void SetupPrefixes()
        {
            /*
            taken from: https://www.freecodecamp.org/news/create-a-solid-to-do-app-with-react/
            @prefix as:    <https://www.w3.org/ns/activitystreams#> .
            @prefix rdf:   <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
            @prefix xsd:   <http://www.w3.org/2001/XMLSchema#> .
            @prefix ldp:   <http://www.w3.org/ns/ldp#> .
            @prefix skos:  <http://www.w3.org/2004/02/skos/core#> .
            @prefix rdfs:  <http://www.w3.org/2000/01/rdf-schema#> .
            @prefix acl:   <http://www.w3.org/ns/auth/acl#> .
            @prefix vcard: <http://www.w3.org/2006/vcard/ns#> .
            @prefix foaf:  <http://xmlns.com/foaf/0.1/> .
            @prefix dc:    <http://purl.org/dc/terms/> .
            @prefix acp:   <http://www.w3.org/ns/solid/acp#> .

            <https://pod.inrupt.com/virginiabalseiro/todos/index.ttl>
                    rdf:type  ldp:RDFSource .
             */

            _graph.NamespaceMap.Clear();

            _graph.NamespaceMap.AddNamespace("as", UriFactory.Create("https://www.w3.org/ns/activitystreams#"));

            // http://www.w3.org/1999/02/22-rdf-syntax-ns
            // added the #type

            _graph.NamespaceMap.AddNamespace("rdf", UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
            _graph.NamespaceMap.AddNamespace("xsd", UriFactory.Create("http://www.w3.org/2001/XMLSchema#"));
            _graph.NamespaceMap.AddNamespace("ldp", UriFactory.Create("http://www.w3.org/ns/ldp#"));
            _graph.NamespaceMap.AddNamespace("skos", UriFactory.Create("http://www.w3.org/2004/02/skos/core#"));
            _graph.NamespaceMap.AddNamespace("rdfs", UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#"));
            _graph.NamespaceMap.AddNamespace("acl", UriFactory.Create("http://www.w3.org/ns/auth/acl#"));
            _graph.NamespaceMap.AddNamespace("vcard", UriFactory.Create("http://www.w3.org/2006/vcard/ns#"));
            _graph.NamespaceMap.AddNamespace("foaf", UriFactory.Create("http://xmlns.com/foaf/0.1/"));
            _graph.NamespaceMap.AddNamespace("dc", UriFactory.Create("http://purl.org/dc/terms/"));
            _graph.NamespaceMap.AddNamespace("acp", UriFactory.Create("http://www.w3.org/ns/solid/acp#"));

            // add the to do namespaces
            _graph.NamespaceMap.AddNamespace("text", UriFactory.Create(ToDo.TextUri));
            _graph.NamespaceMap.AddNamespace("created", UriFactory.Create(ToDo.CreatedUri));
            _graph.NamespaceMap.AddNamespace("type", UriFactory.Create(ToDo.TypeUri));
        }

        /// <summary>
        /// Returns the backing RDF document as a string
        /// </summary>
        /// <returns>The RDF document in a string</returns>
        public override string ToString()
        {
            var writer = new StringWriter();
            _turtleWriter.PrettyPrintMode = true;
            _turtleWriter.Save(_graph, writer);
            return writer.ToString();
        }
        #endregion

        #region Private Methods
        private ToDo GetOrAddItem(List<ToDo> items, int id)
        {
            if (ListHasItem(items, id))
            {
                return GetItem(items, id);
            }
            else
            {
                var item = new ToDo { Id = id };
                items.Add(item);
                return item;
            }
        }

        private bool ListHasItem(List<ToDo> items, int id)
        {
            foreach (var item in items)
            {
                if (item.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private ToDo GetItem(List<ToDo> items, int id)
        {
            foreach (var item in items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private List<ToDo> GetToDosFromGraph()
        {
            var result = new List<ToDo>();

            if (_graph.Triples.Count == 0)
            {
                return new List<ToDo>();
            }
            else
            {
                int currentItem = 0;
                foreach (var triple in _graph.Triples)
                {
                    if (triple.Subject.NodeType == NodeType.Uri)
                    {
                        var uriNode = triple.Subject as UriNode;
                        if (uriNode.Uri.AbsoluteUri.Contains("index.ttl#"))
                        {
                            var idLine = uriNode.Uri.AbsoluteUri;
                            var id = idLine.Replace(_baseUri, string.Empty).Replace("#", string.Empty);

                            int idValue = Convert.ToInt32(id);
                            var item = GetOrAddItem(result, idValue);
                            currentItem = item.Id;
                        }
                    }

                    if (triple.Predicate.NodeType == NodeType.Uri)
                    {
                        var uriNode = triple.Predicate as UriNode;
                        if (uriNode.Uri.AbsoluteUri.Contains("#created"))
                        {
                            if (triple.Object.NodeType == NodeType.Literal)
                            {
                                var literalNode = triple.Object as LiteralNode;
                                var item = GetItem(result, currentItem);
                                item.Created = DateTime.Parse(literalNode.Value);
                            }
                        }

                        if (uriNode.Uri.AbsoluteUri.Contains("text"))
                        {
                            if (triple.Object.NodeType == NodeType.Literal)
                            {
                                var literalNode = triple.Object as LiteralNode;
                                var item = GetItem(result, currentItem);
                                item.Text = literalNode.Value;
                            }
                        }
                    }
                }
            }

            return result;
        }

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
