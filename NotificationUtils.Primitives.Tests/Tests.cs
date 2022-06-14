using NUnit.Framework;

namespace NotificationUtils.Primitives.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test1()
        {
            string propName = null;
            string newValue = null;
            
            var testObj = new DerivedClass()
            {
                Name = "ユウカ"
            };

            testObj.PropertyChanged += (o, e) =>
            {
                propName = e.PropertyName;
                newValue = ((DerivedClass)o).Name;
            };
            
            testObj.SetProperty(nameof(DerivedClass.Name), "ミドリ");
            
            Assert.AreEqual("ミドリ", testObj.Name, "対象のプロパティの値が変更されていること");
            Assert.AreEqual("Name", propName, "変更通知イベントが受け取った引数のプロパティ名が正しいこと");
            Assert.AreEqual("ミドリ", newValue, "変更通知イベントハンドラの中で対象の値が変更されていること");
        }
    }
    
    public class DerivedClass : NotificationObjectBase
    {
        public string Name { get; set; }
    }
}