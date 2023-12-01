using Bogus;

namespace ExtravaWall;

public class ExtravaFake<T> : Faker<T> where T : class {
    private int seed = 0;

    public ExtravaFake(int initialSeed = 0, string locale = "en") : base(locale) {
        this.seed = initialSeed;
    }

    protected override void PopulateInternal(T instance, string[] ruleSets) {
        this.UseSeed(seed++);
        base.PopulateInternal(instance, ruleSets);
    }
}