using System;

namespace AIStudyHub.Models
{
    public class DocumentChunk
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        
        public int ChunkIndex { get; set; }
        
        public string Content { get; set; } = string.Empty;

        public Document? Document { get; set; }
    }
}
