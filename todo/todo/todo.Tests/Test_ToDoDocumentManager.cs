using Xunit;

namespace todo.Tests
{
    public class Test_ToDoDocumentManager
    {
        [Fact]
        public void TestAddItem()
        {
            var doc = new ToDoDocumentManager();
            doc.ConfigureBaseUri("http://localhost/");
            doc.AddToDo(new Data.ToDo { Id = 1, Text = "This is a test", Created = DateTime.Now });
            var result = doc.ToString();
            Assert.Empty(result);
        }
    }
}