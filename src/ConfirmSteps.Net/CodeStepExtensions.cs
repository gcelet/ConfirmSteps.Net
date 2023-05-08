namespace ConfirmSteps;

using ConfirmSteps.Steps.Code;

public static class CodeStepExtensions
{
    public static IStepBuilderAppender<T> CodeStep<T>(this IStepBuilderAppender<T> stepBuilderAppender, string title,
        Action<CodeStepBuilder<T>> stepBuilder)
        where T : class
    {
        CodeStepBuilder<T> codeStepBuilder = new();

        stepBuilder.Invoke(codeStepBuilder);

        stepBuilderAppender.Append(codeStepBuilder);

        return stepBuilderAppender;
    }
}