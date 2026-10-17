using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Helpers;

internal static class AsyncQueryableExtensions
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    {
        return new AsyncEnumerable<T>(source);
    }
}

internal class AsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IEnumerable<T> _inner;
    private readonly IQueryProvider _provider;

    public AsyncEnumerable(IEnumerable<T> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _provider = new AsyncQueryProvider<T>(_inner.AsQueryable().Provider);
    }

    public Type ElementType => typeof(T);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => _provider;

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new AsyncEnumerator<T>(_inner.GetEnumerator());
}

internal class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}

internal class AsyncQueryProvider<T> : IQueryProvider
{
    private readonly IQueryProvider _inner;

    public AsyncQueryProvider(IQueryProvider innerProvider) => _inner = innerProvider;

    public IQueryable CreateQuery(Expression expression)
    {
        return new AsyncEnumerable<T>((IEnumerable<T>)_inner.CreateQuery(expression));
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new AsyncEnumerable<TElement>((IEnumerable<TElement>)_inner.CreateQuery(expression));
    }

    public object Execute(Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
}
