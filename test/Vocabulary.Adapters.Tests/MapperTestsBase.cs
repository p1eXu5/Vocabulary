using AutoMapper;
using p1eXu5.AutoProfile;
using Vocabulary.BlazorServer.Tests.Factories;

namespace Vocabulary.Adapters.Tests;

public abstract class MapperTestsBase
{
    protected IMapper Mapper { get; private set; } = default!;

    protected virtual ICollection<Type> MappingTypes { get; } = new Type[0];

    [OneTimeSetUp]
    public void Initialize()
    {
        AutoProfile autoProfile = new AutoProfile(
            MockLoggerFactories.GetMockILogger<AutoProfile>(TestContext.WriteLine).Object,
            new AutoProfileOptions { NotProcessMapAttributesFromAssembly = true }
        );

        foreach (var type in MappingTypes)
        {
            autoProfile.CreateMaps(type);
        }

        var conf = new MapperConfiguration(cfg => cfg.AddProfile(autoProfile.Configure()));
        conf.AssertConfigurationIsValid();

        Mapper = conf.CreateMapper();
    }
}