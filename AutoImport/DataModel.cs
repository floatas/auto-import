namespace AutoImport
{
    public class DataModel
    {
        public string NewName { get; set; }
        public string TypeName { get; set; }
        public bool IsInterface { get; set; }
        public bool IsGeneric { get; set; }

        public DataModel()
        {
            NewName = string.Empty;
            TypeName = string.Empty;
        }
    }
}
