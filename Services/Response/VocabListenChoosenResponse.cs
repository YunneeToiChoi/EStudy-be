﻿namespace study4_be.Services.Response
{
    public class VocabListenChoosenResponse
    {
        public int vocabId { get; set; }    
        public string vocabTitle {get; set;}
        public string vocabMean { get; set; }
        public byte[] vocabAudioBytes { get; set; }
    }
}
