using todo.Data;
using VDS.RDF;
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
        /// <exception cref="NotImplementedException"></exception>
        public List<ToDo> ParseDocument(string rdfText)
        {
            throw new NotImplementedException();
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
        #endregion
    }
}
