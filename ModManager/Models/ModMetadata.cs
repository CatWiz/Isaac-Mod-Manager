using System.Collections.Generic;
using System.Xml.Serialization;

namespace ModManager.Models;

[XmlRoot("metadata")]
public record ModMetadata
{
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("directory")]
    public string Directory { get; set; } = string.Empty;

    [XmlElement("id")]
    public long Id { get; set; }

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;

    [XmlElement("visibility")]
    public string Visibility { get; set; } = string.Empty;

    [XmlElement("tag")]
    public List<Tag> Tags { get; set; } = new List<Tag>();
}

public record Tag
{
    [XmlAttribute("id")]
    public string Name { get; set; } = string.Empty;
}