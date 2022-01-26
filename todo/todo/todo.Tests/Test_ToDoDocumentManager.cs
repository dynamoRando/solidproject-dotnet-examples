using Xunit;

namespace todo.Tests
{
    public class Test_ToDoDocumentManager
    {
        [Fact]
        public void TestAddItem()
        {
            var doc = new ToDoDocumentManager();
            doc.ConfigureBaseUri("http://localhost:3000/");
            doc.AddToDo(new Data.ToDo { Id = 1, Text = "Finish the Solid Todo App tutorial", Created = DateTime.Now });
            var result = doc.ToString();
            Assert.Empty(result);
        }
    }
}