using BusinessObjects.Models;
using Repositories;

namespace Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public List<Tag> GetTags() => _tagRepository.GetTags();
}
