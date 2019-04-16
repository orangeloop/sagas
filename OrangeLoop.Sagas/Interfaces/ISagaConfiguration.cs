using System.Collections.Generic;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface ISagaConfiguration<K>
    {
        LinkedList<ISagaStep<K>> Steps { get; }
        ISagaStep<K> AddStep(ISagaStep<K> step);
        ISagaStep<K> AddStep(K step);
    }
}
