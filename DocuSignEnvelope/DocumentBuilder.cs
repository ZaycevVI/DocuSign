using System;
using System.Collections.Generic;
using System.IO;
using DocuSign.eSign.Model;

public class DocumentBuilder
{
    private int _documentId = 1;
    private readonly List<Document> _documents = new List<Document>();

    public static DocumentBuilder Builder
        => new DocumentBuilder();

    public DocumentBuilder WithDocument(string path)
    {
        var buffer = File.ReadAllBytes(path);
        var docB64 = Convert.ToBase64String(buffer);

        _documents.Add(new Document
        {
            DocumentBase64 = docB64,
            Name = $"DocuSign Document #{_documentId}",
            FileExtension = Path.GetExtension(path),
            DocumentId = _documentId.ToString()
        });

        _documentId++;

        return this;
    }

    public List<Document> Build()
    {
        return _documents;
    }
}