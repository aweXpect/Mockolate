namespace Mockolate.SourceGenerators.Entities;

internal record ObsoleteAttribute
{
	public ObsoleteAttribute(string? Text)
	{
		this.Text = Text;
	}

	public string? Text { get; }
}
