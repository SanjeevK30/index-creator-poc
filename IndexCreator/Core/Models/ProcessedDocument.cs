using Azure.Search.Documents.Indexes;

namespace IndexCreator.Core.Models
{
    public class ProcessedDocument
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; }

        [SearchableField]
        public string Content { get; set; }

        [SearchableField]
        public float[] ContentVector { get; set; }

        [SimpleField]        
        public string Meta_json_string { get; set; }

        [SimpleField]
        public string Filepath { get; set; }

        [SearchableField(IsSortable = true)]
        public string Title { get; set; }

    }
}
