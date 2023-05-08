namespace ConfirmSteps;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.RequestBuilding;

public static class HttpStepExtensions
{
    public static IStepBuilderAppender<T> HttpStep<T>(this IStepBuilderAppender<T> stepBuilderAppender, string title,
        Func<RequestBuilder> requestBuilder,
        Action<HttpStepBuilder<T>>? stepBuilder = null)
        where T : class
    {
        HttpStepBuilder<T> httpStepBuilder = new(requestBuilder);

        stepBuilder?.Invoke(httpStepBuilder);

        stepBuilderAppender.Append(httpStepBuilder);

        return stepBuilderAppender;
    }
}