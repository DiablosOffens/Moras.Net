using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Moras.Net.IndyCustom
{
    internal static class ClassMethodHelper<T>
    {
        private static readonly object s_classType = new Borland.Vcl.TTypeData(typeof(T)).ClassType();

        private static MethodInfo GetMethodInfo<TResult>(Expression<Func<TResult>> exp)
        {
            if (exp == null)
                throw new ArgumentNullException("exp");
            MethodCallExpression mexp = exp.Body as MethodCallExpression;
            if (mexp == null)
                throw new ArgumentException("The lambda expression can only contain a method call.", "exp");
            return mexp.Method;
        }

        private static MethodInfo GetMethodInfo(Expression<Action> exp)
        {
            if (exp == null)
                throw new ArgumentNullException("exp");
            MethodCallExpression mexp = exp.Body as MethodCallExpression;
            if (mexp == null)
                throw new ArgumentException("The lambda expression can only contain a method call.", "exp");
            return mexp.Method;
        }

        internal static Action CreateClassProcDelegate(Expression<Action> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Action)Delegate.CreateDelegate(typeof(Action), s_classType, miClassMethod);
        }

        internal static Action<T2> CreateClassProcDelegate<T2>(Expression<Action> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Action<T2>)Delegate.CreateDelegate(typeof(Action<T2>), s_classType, miClassMethod);
        }

        internal static Action<T2, T3> CreateClassProcDelegate<T2, T3>(Expression<Action> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Action<T2, T3>)Delegate.CreateDelegate(typeof(Action<T2, T3>), s_classType, miClassMethod);
        }

        internal static Action<T2, T3, T4> CreateClassProcDelegate<T2, T3, T4>(Expression<Action> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Action<T2, T3, T4>)Delegate.CreateDelegate(typeof(Action<T2, T3, T4>), s_classType, miClassMethod);
        }

        internal static Action<T2, T3, T4, T5> CreateClassProcDelegate<T2, T3, T4, T5>(Expression<Action> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Action<T2, T3, T4, T5>)Delegate.CreateDelegate(typeof(Action<T2, T3, T4, T5>), s_classType, miClassMethod);
        }

        internal static Action<T2, T3, T4, T5, T6> CreateClassProcDelegate<T2, T3, T4, T5, T6>(Expression<Action> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Action<T2, T3, T4, T5, T6>)Delegate.CreateDelegate(typeof(Action<T2, T3, T4, T5, T6>), s_classType, miClassMethod);
        }

        internal static Func<TResult> CreateClassFuncDelegate<TResult>(Expression<Func<TResult>> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Func<TResult>)Delegate.CreateDelegate(typeof(Func<TResult>), s_classType, miClassMethod);
        }

        internal static Func<T2, TResult> CreateClassFuncDelegate<T2, TResult>(Expression<Func<TResult>> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Func<T2, TResult>)Delegate.CreateDelegate(typeof(Func<T2, TResult>), s_classType, miClassMethod);
        }

        internal static Func<T2, T3, TResult> CreateClassFuncDelegate<T2, T3, TResult>(Expression<Func<TResult>> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Func<T2, T3, TResult>)Delegate.CreateDelegate(typeof(Func<T2, T3, TResult>), s_classType, miClassMethod);
        }

        internal static Func<T2, T3, T4, TResult> CreateClassFuncDelegate<T2, T3, T4, TResult>(Expression<Func<TResult>> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Func<T2, T3, T4, TResult>)Delegate.CreateDelegate(typeof(Func<T2, T3, T4, TResult>), s_classType, miClassMethod);
        }

        internal static Func<T2, T3, T4, T5, TResult> CreateClassFuncDelegate<T2, T3, T4, T5, TResult>(Expression<Func<TResult>> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Func<T2, T3, T4, T5, TResult>)Delegate.CreateDelegate(typeof(Func<T2, T3, T4, T5, TResult>), s_classType, miClassMethod);
        }

        internal static Func<T2, T3, T4, T5, T6, TResult> CreateClassFuncDelegate<T2, T3, T4, T5, T6, TResult>(Expression<Func<TResult>> exp)
        {
            MethodInfo miClassMethod = GetMethodInfo(exp);
            return (Func<T2, T3, T4, T5, T6, TResult>)Delegate.CreateDelegate(typeof(Func<T2, T3, T4, T5, T6, TResult>), s_classType, miClassMethod);
        }

        internal static void CallProcWithClassType(Expression<Action> expClassMethod)
        {
            Action classMethod = CreateClassProcDelegate(expClassMethod);
            classMethod();
        }

        internal static void CallProcWithClassType<T2>(Expression<Action> expClassMethod, T2 arg2)
        {
            Action<T2> classMethod = CreateClassProcDelegate<T2>(expClassMethod);
            classMethod(arg2);
        }

        internal static void CallProcWithClassType<T2, T3>(Expression<Action> expClassMethod, T2 arg2, T3 arg3)
        {
            Action<T2, T3> classMethod = CreateClassProcDelegate<T2, T3>(expClassMethod);
            classMethod(arg2, arg3);
        }

        internal static void CallProcWithClassType<T2, T3, T4>(Expression<Action> expClassMethod, T2 arg2, T3 arg3, T4 arg4)
        {
            Action<T2, T3, T4> classMethod = CreateClassProcDelegate<T2, T3, T4>(expClassMethod);
            classMethod(arg2, arg3, arg4);
        }

        internal static void CallProcWithClassType<T2, T3, T4, T5>(Expression<Action> expClassMethod, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Action<T2, T3, T4, T5> classMethod = CreateClassProcDelegate<T2, T3, T4, T5>(expClassMethod);
            classMethod(arg2, arg3, arg4, arg5);
        }

        internal static void CallProcWithClassType<T2, T3, T4, T5, T6>(Expression<Action> expClassMethod, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Action<T2, T3, T4, T5, T6> classMethod = CreateClassProcDelegate<T2, T3, T4, T5, T6>(expClassMethod);
            classMethod(arg2, arg3, arg4, arg5, arg6);
        }

        internal static TResult CallFuncWithClassType<TResult>(Expression<Func<TResult>> expClassMethod)
        {
            Func<TResult> classMethod = CreateClassFuncDelegate(expClassMethod);
            return classMethod();
        }

        internal static TResult CallFuncWithClassType<T2, TResult>(Expression<Func<TResult>> expClassMethod, T2 arg2)
        {
            Func<T2, TResult> classMethod = CreateClassFuncDelegate<T2, TResult>(expClassMethod);
            return classMethod(arg2);
        }

        internal static TResult CallFuncWithClassType<T2, T3, TResult>(Expression<Func<TResult>> expClassMethod, T2 arg2, T3 arg3)
        {
            Func<T2, T3, TResult> classMethod = CreateClassFuncDelegate<T2, T3, TResult>(expClassMethod);
            return classMethod(arg2, arg3);
        }

        internal static TResult CallFuncWithClassType<T2, T3, T4, TResult>(Expression<Func<TResult>> expClassMethod, T2 arg2, T3 arg3, T4 arg4)
        {
            Func<T2, T3, T4, TResult> classMethod = CreateClassFuncDelegate<T2, T3, T4, TResult>(expClassMethod);
            return classMethod(arg2, arg3, arg4);
        }

        internal static TResult CallFuncWithClassType<T2, T3, T4, T5, TResult>(Expression<Func<TResult>> expClassMethod, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Func<T2, T3, T4, T5, TResult> classMethod = CreateClassFuncDelegate<T2, T3, T4, T5, TResult>(expClassMethod);
            return classMethod(arg2, arg3, arg4, arg5);
        }

        internal static TResult CallFuncWithClassType<TMeta, T2, T3, T4, T5, T6, TResult>(Expression<Func<TResult>> expClassMethod, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Func<T2, T3, T4, T5, T6, TResult> classMethod = CreateClassFuncDelegate<T2, T3, T4, T5, T6, TResult>(expClassMethod);
            return classMethod(arg2, arg3, arg4, arg5, arg6);
        }
    }
}
