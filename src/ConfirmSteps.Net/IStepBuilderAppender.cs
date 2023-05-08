namespace ConfirmSteps;

using ConfirmSteps.Internal;

public interface IStepBuilderAppender<T>
    where T : class
{
    void Append(IStepBuilder<T> stepBuilder);
}