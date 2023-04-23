using System;
using System.Collections.Generic;
using UniRx.Operators;

namespace UniRx.Operators
{
    internal class ReduceObservable<T> : OperatorObservableBase<IList<T>>
    {
        readonly IObservable<T> source;
        readonly TimeSpan timeSpan;
        readonly IScheduler scheduler;

        public ReduceObservable(IObservable<T> source, TimeSpan timeSpan, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread || source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.timeSpan = timeSpan;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(IObserver<IList<T>> observer, IDisposable cancel)
        {
            return new ReduceT(this, observer, cancel).Run();
        }

        // timespan = timeshift
        class ReduceT : OperatorObserverBase<T, IList<T>>
        {
            static readonly T[] EmptyArray = new T[0];

            readonly ReduceObservable<T> parent;
            readonly object gate = new object();
            List<T> list;

            public ReduceT(ReduceObservable<T> parent, IObserver<IList<T>> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                list = new List<T>();

                var timerSubscription = Observable.Interval(parent.timeSpan, parent.scheduler)
                    .Subscribe(new Reduce(this));
                var sourceSubscription = parent.source.Subscribe(this);
                return StableCompositeDisposable.Create(timerSubscription, sourceSubscription);
            }

            public override void OnNext(T value)
            {
                lock (gate)
                {
                    if (list.Contains(value))
                    {
                        return;
                    }
                    list.Add(value);
                }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); } finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                List<T> currentList;
                lock (gate)
                {
                    currentList = list;
                }
                observer.OnNext(currentList);
                try { observer.OnCompleted(); } finally { Dispose(); }
            }

            class Reduce : IObserver<long>
            {
                ReduceT parent;

                public Reduce(ReduceT parent)
                {
                    this.parent = parent;
                }

                public void OnNext(long value)
                {
                    var isZero = false;
                    List<T> currentList;
                    lock(parent.gate)
                    {
                        currentList = parent.list;
                        if (currentList.Count != 0)
                        {
                            parent.list = new List<T>();
                        }
                        else
                        {
                            isZero = true;
                        }
                    }
                    parent.observer.OnNext((isZero) ? (IList<T>)EmptyArray : currentList);
                }
                public void OnError(Exception error){}
                public void OnCompleted(){}
            }
        }
    }

    internal class ReduceObservable<TSource, TWindowBoundary> : OperatorObservableBase<IList<TSource>>
    {
        readonly IObservable<TSource> source;
        readonly IObservable<TWindowBoundary> windowBoundaries;

        public ReduceObservable(IObservable<TSource> source, IObservable<TWindowBoundary> windowBoundaries)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.windowBoundaries = windowBoundaries;
        }

        protected override IDisposable SubscribeCore(IObserver<IList<TSource>> observer, IDisposable cancel)
        {
            return new Reduce(this, observer, cancel).Run();
        }

        class Reduce : OperatorObserverBase<TSource, IList<TSource>>
        {
            static readonly TSource[] EmptyArray = new TSource[0]; // cache

            readonly ReduceObservable<TSource, TWindowBoundary> parent;
            object gate = new object();
            List<TSource> list;

            public Reduce(ReduceObservable<TSource, TWindowBoundary> parent, IObserver<IList<TSource>> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                list = new List<TSource>();

                var sourceSubscription = parent.source.Subscribe(this);
                var windowSubscription = parent.windowBoundaries.Subscribe(new Reduce_(this));

                return StableCompositeDisposable.Create(sourceSubscription, windowSubscription);
            }

            public override void OnNext(TSource value)
            {
                lock (gate)
                {
                    if (list.Contains(value))
                    {
                        return;
                    }
                    list.Add(value);
                }
            }

            public override void OnError(Exception error)
            {
                lock (gate)
                {
                    try { observer.OnError(error); } finally { Dispose(); }
                }
            }

            public override void OnCompleted()
            {
                lock (gate)
                {
                    var currentList = list;
                    list = new List<TSource>(); // safe
                    observer.OnNext(currentList);
                    try { observer.OnCompleted(); } finally { Dispose(); }
                }
            }

            class Reduce_ : IObserver<TWindowBoundary>
            {
                readonly Reduce parent;

                public Reduce_(Reduce parent)
                {
                    this.parent = parent;
                }

                public void OnNext(TWindowBoundary value)
                {
                    var isZero = false;
                    List<TSource> currentList;
                    lock (parent.gate)
                    {
                        currentList = parent.list;
                        if (currentList.Count != 0)
                        {
                            parent.list = new List<TSource>();
                        }
                        else
                        {
                            isZero = true;
                        }
                    }
                    if (isZero)
                    {
                        parent.observer.OnNext(EmptyArray);
                    }
                    else
                    {
                        parent.observer.OnNext(currentList);
                    }
                }

                public void OnError(Exception error)
                {
                    parent.OnError(error);
                }

                public void OnCompleted()
                {
                    parent.OnCompleted();
                }
            }
        }
    }
}