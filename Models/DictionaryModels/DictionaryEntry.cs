using LexiFlow.Models.PronunciationModels;

namespace LexiFlow.Models.DictionaryModels;

public class DictionaryEntry
{
    public string Word { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Origin { get; set; } = "";
    public List<PronunciationVariant> Pronunciations { get; set; } = [];
    public List<DictionaryPartOfSpeech> Meanings { get; set; } = [];
    public List<TranslationItem> Translations { get; set; } = [];
    public List<string> Synonyms { get; set; } = [];
    public List<string> Antonyms { get; set; } = [];
    public List<DictionaryLink> PhrasalVerbs { get; set; } = [];
    public List<DictionaryLink> Idioms { get; set; } = [];
    public List<DictionaryLink> Collocations { get; set; } = [];
    public DictionaryDialectValues? Ipa { get; set; }
    public DictionaryDialectValues? Audio { get; set; }
}