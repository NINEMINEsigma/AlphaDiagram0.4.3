using System;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;

namespace AD.Utility.Pipe
{
    public class PipeContext
    {
        public object Head { get; internal set; }
        internal HashSet<object> Objects = new();

        public PipeContext Link(object _Right)
        {
            Objects.Add(_Right);
            return this;
        }

        public HashSet<object> ToHashSet()
        {
            return Objects;
        }

        public HashSet<P> ToHashSet<P>() where P : class
        {
            HashSet<P> result = new();
            foreach (var item in Objects)
                if (item is P r)
                    result.Add(r);
            return result;
        }
    }

    public class PipeContext<T>
    {
        public T Head { get; internal set; }
        internal List<T> Objects = new();

        public PipeContext<T> Link(T _Right)
        {
            Objects.Add(_Right);
            return this;
        }

        public List<T> ToList()
        {
            return Objects;
        }

        public List<P> ToList<P>() where P : class
        {
            List<P> result = new();
            foreach (var item in Objects)
                if (item is P r)
                    result.Add(r);
            return result;
        }
    }

    public class PipeContext<Key, Value>
    {
        public KeyValuePair<Key, Value> Head { get; internal set; }
        internal Dictionary<Key, Value> Objects = new();

        public PipeContext<Key, Value> Link((Key, Value) _Right)
        {
            Objects[_Right.Item1] = _Right.Item2;
            return this;
        }

        public PipeContext<Key, Value> Link(KeyValuePair<Key, Value> _Right)
        {
            Objects[_Right.Key] = _Right.Value;
            return this;
        }

        public PipeContext<Key, Value> Link(Key key, Value value)
        {
            Objects[key] = value;
            return this;
        }

        public Dictionary<Key, Value> ToDictionary()
        {
            return Objects;
        }

        public Dictionary<Key, P> ToDictionary<P>() where P : class
        {
            Dictionary<Key, P> result = new();
            foreach (var item in Objects)
                if (item.Value is P r)
                    result[item.Key] = r;
            return result;
        }

        public List<Value> ToList()
        {
            List<Value> result = new();
            foreach (var item in Objects)
                result.Add(item.Value);
            return result;
        }

        public List<P> ToList<P>() where P : class
        {
            List<P> result = new();
            foreach (var item in Objects)
                if (item.Value is P r)
                    result.Add(r);
            return result;
        }

    }

    public sealed class PipeProperty<T>
    {
        internal PipeProperty(T org)
        {
            target = org;
            pipeLineSteps = new();
        }

        public T target;

        internal List<IPipeLineStep> pipeLineSteps;
        internal bool IsBuilding = false;

        internal List<IPipeLineStep> PIPELINE;

        private List<(IPipeLineIf,List<IPipeLineStep>)> PredicateAndSteps = null; 
        private int currentPredicate = -1;
        private Type currentOperationalPipeLineSteps_OUTPUT = null;

        internal object GetValue()
        {
            if (IsBuilding) throw new ADException("this property is still building");
            if (PIPELINE == null) return null;
            object current = target;
            for (int i = 0; i < PIPELINE.Count; i++)
            {
                IPipeLineStep step = PIPELINE[i];
                current = Convert.ChangeType(step.Execute(Convert.ChangeType(target, step._INPUT)), step._OUTPUT);
            }
            return current;
        }

        public void SharedPIPELINETo(PipeProperty<T> other)
        {
            other.PIPELINE = this.PIPELINE;
        }

        public PipeProperty<T> Begin()
        {
            if (this.IsBuilding) throw new ADException(
                "The pipeline didn't end the build and you tried to start building again, " +
                "or you didn't start building and you tried to end the build");
            this.IsBuilding = true;
            this.pipeLineSteps.Clear();
            return this;
        }

        public PipeProperty<T> Step(IPipeLineStep step)
        {
            return currentPredicate == -1 ? StepOnMainStream(step) : StepOnIfStream(step);
        }

        public Type GetCurrentFinalOuputType()
        {
            if (currentPredicate == -1)
            {
                return this.pipeLineSteps.Count > 0 ? this.pipeLineSteps[^1]._OUTPUT : typeof(T);
            }
            else
            {
                return this.PredicateAndSteps[currentPredicate].Item2.Count > 0 ? this.PredicateAndSteps[currentPredicate].Item2[^1]._OUTPUT : 
                    (this.pipeLineSteps.Count > 0 ? this.pipeLineSteps[^1]._OUTPUT : typeof(T));
            }
        }

        private PipeProperty<T> StepOnIfStream(IPipeLineStep step)
        {
            if (!this.IsBuilding) throw new ADException(
                "The pipeline has finished building or has not started to build, and you tried to add steps to it");
            if (this.PredicateAndSteps[currentPredicate].Item2.Count > 0 && !GetCurrentFinalOuputType().IsAssignableFromOrSubClass(step._INPUT))
                throw new ADException("Pipeline steps must be sequential and of the same type, and do not support inversion or covariance");
            this.PredicateAndSteps[currentPredicate].Item2.Add(step);
            return this;
        }

        private PipeProperty<T> StepOnMainStream(IPipeLineStep step)
        {
            if (!this.IsBuilding) throw new ADException(
                "The pipeline has finished building or has not started to build, and you tried to add steps to it");
            if (this.pipeLineSteps.Count > 0 && !GetCurrentFinalOuputType().IsAssignableFromOrSubClass(step._INPUT))
                throw new ADException("Pipeline steps must be sequential and of the same type, and do not support inversion or covariance");
            this.pipeLineSteps.Add(step);
            return this;
        }

        public PipeProperty<T> Begin<_INPUT, _OUTPUT>()
        {
            if (this.IsBuilding) throw new ADException(
                "The pipeline didn't end the build and you tried to start building again, " +
                "or you didn't start building and you tried to end the build");
            this.IsBuilding = true;
            this.pipeLineSteps.Clear();
            return this;
        }

        public PipeProperty<T> Step<_INPUT, _OUTPUT>(Func<_INPUT, _OUTPUT> step) where _INPUT : class
        {
            return Step(new PipeFunc<_INPUT, _OUTPUT>(step));
        }

        public PipeProperty<T> End()
        {
            if (!this.IsBuilding) throw new ADException(
                "The pipeline didn't end the build and you tried to start building again, " +
                "or you didn't start building and you tried to end the build");
            this.IsBuilding = false;
            this.PIPELINE = this.pipeLineSteps;
            this.pipeLineSteps = new();
            return this;
        }

        public PipeProperty<T> If<_FinalOutput>(IPipeLineIf PredicateRight)
        {
            if (PredicateAndSteps != null) EndIf();
            else PredicateAndSteps = new();
            MakeNewPredicateRightOnIfStream(PredicateRight);
            currentOperationalPipeLineSteps_OUTPUT = typeof(_FinalOutput);
            return this;
        }

        public PipeProperty<T> ElseIf(IPipeLineIf PredicateRight)
        {
            CheakIfStreamFinalOutput();
            MakeNewPredicateRightOnIfStream(PredicateRight);
            return this;
        }

        public PipeProperty<T> Else()
        {
            var cat = new PipeLineIfAlwaysTure(pipeLineSteps[^1]._OUTPUT);
            CheakIfStreamFinalOutput();
            MakeNewPredicateRightOnIfStream(cat);
            return this;
        }

        public PipeProperty<T> EndIf()
        {
            CheakIfStreamFinalOutput();
            InitAndEndIf();
            return this;
        }

        //TODO

        public PipeProperty<T> While<_INPUT>(Predicate<_INPUT> predicate)
        {
            return this;
        }

        public PipeProperty<T> EndWhile<_INPUT>()
        {
            return this;
        }


        //ENDTODO

        private void InitAndEndIf()
        { 
            currentPredicate = -1;
            currentOperationalPipeLineSteps_OUTPUT = null;
            pipeLineSteps.Add(new BuildingIfStreamClass(PredicateAndSteps, pipeLineSteps[^1]._OUTPUT, currentOperationalPipeLineSteps_OUTPUT));
            PredicateAndSteps = null;
        }

        public class BuildingIfStreamClass: IPipeLineStep
        {
            public object Execute(object _Right)
            {
                foreach (var StepBranch in PredicateAndSteps)
                {
                    if (StepBranch.Item1.Predicate(_Right))
                    {
                        object current = _Right;
                        foreach (var Step in StepBranch.Item2)
                        {
                            current = Step.Execute(current);
                        }
                        return current;
                    }
                }
                return _Right;
            }

            private List<(IPipeLineIf, List<IPipeLineStep>)> PredicateAndSteps = null; 

            public Type _INPUT { get; set; }

            public Type _OUTPUT { get; set; }

            public BuildingIfStreamClass(List<(IPipeLineIf, List<IPipeLineStep>)> predicateAndSteps, Type input, Type output)
            {
                PredicateAndSteps = predicateAndSteps;
                _INPUT = input;
                _OUTPUT = output;
            }
        }

        private void MakeNewPredicateRightOnIfStream(IPipeLineIf PredicateRight)
        {
            currentPredicate = PredicateAndSteps.Count;
            PredicateAndSteps.Add((PredicateRight, new()));
        }

        private bool CheakIfStreamFinalOutput()
        {
            return PredicateAndSteps[currentPredicate].Item2.Count == 0 || PredicateAndSteps[currentPredicate].Item2[^1]._OUTPUT == currentOperationalPipeLineSteps_OUTPUT;
        }

        public bool Result<_Result>(out _Result result, bool IsMightNull = false) where _Result : class
        {
            result = null;
            if (this.IsBuilding) this.End();
            var cat = this.GetValue();
            if (cat == null) return IsMightNull;
            result = cat as _Result;
            return cat is _Result;
        }

        public bool PipeEndAndResult<_Result>(out _Result result, bool IsMightNull = false) where _Result : class
        {
            return this.End().Result(out result, IsMightNull);
        }
    }

    public interface IPipeLineStep
    {
        object Execute(object _Right);
        Type _INPUT { get; }
        Type _OUTPUT { get; }
    }
    public interface IPipeLineStep<_INPUT, _OUTPUT>
    {
        _OUTPUT Execute(_INPUT _Right);
    }

    public interface IPipeLineIf
    {
        bool Predicate(object _Right);
        Type _INPUT { get; }
    }

    public class PipeLineIfAlwaysTure: IPipeLineIf
    {
        public PipeLineIfAlwaysTure(Type _INPUT)
        {
            this._INPUT = _INPUT;
        }

        public Type _INPUT { get; private set; }

        public bool Predicate(object _Right)
        {
            return true;
        }
    }

    public class PipeFunc : IPipeLineStep
    {
        public PipeFunc(Func<object, object> step, Type _INPUT, Type _OUTPUT)
        {
            this.step = step;
            this._INPUT = _INPUT;
            this._OUTPUT = _OUTPUT;
        }

        Func<object, object> step;

        public Type _INPUT { get; set; }

        public Type _OUTPUT { get; set; }

        public object Execute(object _Right)
        {
            return step(_Right);
        }
    }

    public class PipeFunc<__INPUT, __OUTPUT> : IPipeLineStep, IPipeLineStep<__INPUT, __OUTPUT>
        where __INPUT : class
    {
        public PipeFunc(Func<__INPUT, __OUTPUT> step)
        {
            this.step = step;
        }

        Func<__INPUT, __OUTPUT> step;

        public Type _INPUT => typeof(__INPUT);

        public Type _OUTPUT => typeof(__OUTPUT);

        public object Execute(object _Right)
        {
            return step(_Right as __INPUT);
        }

        public __OUTPUT Execute(__INPUT _Right)
        {
            return step(_Right);
        }
    }

    public static class ObjectPipe
    {
        public static PipeContext Link(this object self, object _Right)
        {
            PipeContext context = new();
            context.Head = self;
            context.Link(self);
            context.Link(_Right);
            return context;
        }

        public static PipeContext<T> Link<T>(this T self, T _Right) where T : class
        {
            PipeContext<T> context = new();
            context.Head = self;
            context.Link(self);
            context.Link(_Right);
            return context;
        }

        public static PipeContext<Key, Value> Link<Key, Value>(this (Key, Value) self, Key key, Value value)
        {
            PipeContext<Key, Value> context = new();
            context.Link(self);
            context.Head = context.Objects.First();
            context.Link(key, value);
            return context;
        }

        public static PipeContext<Key, Value> Link<Key, Value>(this (Key, Value) self, KeyValuePair<Key, Value> _Right)
        {
            PipeContext<Key, Value> context = new();
            context.Link(self);
            context.Head = context.Objects.First();
            context.Link(_Right.Key, _Right.Value);
            return context;
        }

        public static PipeContext<Key, Value> Link<Key, Value>(this KeyValuePair<Key, Value> self, Key key, Value value)
        {
            PipeContext<Key, Value> context = new();
            context.Link(self);
            context.Head = context.Objects.First();
            context.Link(key, value);
            return context;
        }

        public static PipeContext<Key, Value> Link<Key, Value>(this KeyValuePair<Key, Value> self, KeyValuePair<Key, Value> _Right)
        {
            PipeContext<Key, Value> context = new();
            context.Link(self);
            context.Head = context.Objects.First();
            context.Link(_Right.Key, _Right.Value);
            return context;
        }

        public static PipeProperty<T> Pipe<T>(this T self)
        {
            return new PipeProperty<T>(self);
        }

        public static PipeProperty<T> PipeBegin<T>(this T self, IPipeLineStep step)
        {
            return new PipeProperty<T>(self).Begin().Step(step);
        }

        public static PipeProperty<T> PipeBegin<T, _INPUT, _OUTPUT>(this T self, Func<_INPUT, _OUTPUT> step) where _INPUT : class
        {
            return new PipeProperty<T>(self).Begin().Step(step);
        }

        public static _OUTPUT Step<_INPUT, _OUTPUT>(this _INPUT self, IPipeLineStep<_INPUT, _OUTPUT> step)
        {
            return step.Execute(self);
        }

        public static _OUTPUT Step<_INPUT, _OUTPUT>(this _INPUT self, Func<_INPUT, _OUTPUT> step)
        {
            return step(self);
        }

        public static _OUTPUT Step<_Right, _OUTPUT, _INPUT>(this _INPUT self) where _Right : IPipeLineStep<_INPUT, _OUTPUT>, new()
        {
            return new _Right().Execute(self);
        }

        public static List<IPipeLineStep> Step(this List<IPipeLineStep> self, IPipeLineStep _Right)
        {
            self.Add(_Right);
            return self;
        }

        public static List<IPipeLineStep> Step<_INPUT, _OUTPUT>(this List<IPipeLineStep> self, Func<_INPUT, _OUTPUT> _Right) where _INPUT : class
        {
            self.Add(new PipeFunc<_INPUT, _OUTPUT>(_Right));
            return self;
        }

        public static void CopyPipeLine(this List<IPipeLineStep> self, out List<IPipeLineStep> target)
        {
            target = new();
            foreach (var item in self)
                target.Add(item);
        }
    }
}
