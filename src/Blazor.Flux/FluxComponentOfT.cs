//using Microsoft.AspNetCore.Components;
//using Sg.States;

//namespace Blazor.Flux;

//public abstract class FluxComponent<TComponent> : FluxComponentBase where TComponent : ComponentBase
//{
//    public static class Binding<TState>
//    {
//#pragma warning disable IDE0060
//        public static TState Local(TState state) => default!;
//        public static TState Bind(ChangeTrackingScope scope = ChangeTrackingScope.RootChanged) => default!;
//        public static TValue Bind<TValue>(Func<TState, TValue> selector, ChangeTrackingScope scope = ChangeTrackingScope.RootChanged) => default!;
//        public static TValue Bind<TValue>(Func<TComponent, TState, TValue> selector, ChangeTrackingScope scope = ChangeTrackingScope.RootChanged) => default!;
//        public static TTransform Bind<TValue, TTransform>(Func<TState, TValue> selector, Func<TValue, TTransform> transform, ChangeTrackingScope scope = ChangeTrackingScope.RootChanged) => default!;
//        public static TTransform Bind<TValue, TTransform>(Func<TComponent, TState, TValue> selector, Func<TValue, TTransform> transform, ChangeTrackingScope scope = ChangeTrackingScope.RootChanged) => default!;
//#pragma warning restore IDE0060
//    }
//}
