﻿using System.Collections.Generic;

namespace Furesoft.PrattParser.Text;

public class SourceDocument
{
    public string Filename { get; set; }
    public string Source { get; set; }
    public List<Message> Messages { get; } = new();
}