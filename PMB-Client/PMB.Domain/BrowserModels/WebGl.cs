namespace PMB.Domain.BrowserModels;

public class WebGl
{
    public Noise Noise { get; set; }

    public string Status { get; set; }

    public Params Params { get; set; }

    public bool HideWebGL { get; set; }

    public string Renderer { get; set; }

    public string Vendor { get; set; }

    public string StatusForUser { get; set; }
}

public class Noise
{
    public int Index { get; set; }

    public double Difference { get; set; }
}

public class Params
{
    public _37445 _37445 { get; set; }

    public _37446 _37446 { get; set; }
}

public class _37445
{
    public int Key { get; set; }

    public string Value { get; set; }
}

public class _37446
{
    public int Key { get; set; }

    public string Value { get; set; }
}
