namespace todo.Data
{
    /// <summary>
    /// Represents a To Do item, as plain old CLR object (POCO)
    /// </summary>
    public class ToDo
    {
        public const string TextUri = "http://schema.org/text";
        public const string CreatedUri = "http://www.w3.org/2002/12/cal/ical#created";
        public const string TypeUri = "http://www.w3.org/2002/12/cal/ical#Vtodo";

        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
    }
}
