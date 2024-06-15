using AsyncWorkflow.Interfaces;

namespace SampleAPI.Models;

public class Document : ITrackedPayload<string>
{
	public string Filename { get; set; } = default!;

	public string Key => Filename;
}
