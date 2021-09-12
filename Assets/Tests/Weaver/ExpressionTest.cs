using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Weaver
{
    public class ExpressionTest
    {
        [Test]
        public void MethodCallExpressionAreEqual()
        {
            LambdaExpression lambda1 = (Expression<Action>)(() => ZigZag.Encode(default));
            LambdaExpression lambda2 = (Expression<Action>)(() => ZigZag.Encode(default));
            LambdaExpression lambda3 = (Expression<Action>)(() => ZigZag.Decode(default));

            Assert.That(lambda1.Body, Is.InstanceOf<MethodCallExpression>());

            object key1 = (lambda1.Body as MethodCallExpression).Method;
            object key2 = (lambda2.Body as MethodCallExpression).Method;
            object key3 = (lambda3.Body as MethodCallExpression).Method;

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
            Assert.That(key1.GetHashCode(), Is.Not.EqualTo(key3.GetHashCode()));
        }

        [Test]
        public void MemberExpressionAreEqual()
        {
            LambdaExpression lambda1 = (Expression<Func<NetworkBehaviour, bool>>)((NetworkBehaviour nb) => nb.IsServer);
            LambdaExpression lambda2 = (Expression<Func<NetworkBehaviour, bool>>)((NetworkBehaviour nb) => nb.IsServer);
            LambdaExpression lambda3 = (Expression<Func<NetworkBehaviour, bool>>)((NetworkBehaviour nb) => nb.IsClient);

            Assert.That(lambda1.Body, Is.InstanceOf<MemberExpression>());
            Assert.That((lambda1.Body as MemberExpression).Member, Is.InstanceOf<PropertyInfo>());

            object key1 = ((lambda1.Body as MemberExpression).Member as PropertyInfo).GetMethod;
            object key2 = ((lambda2.Body as MemberExpression).Member as PropertyInfo).GetMethod;
            object key3 = ((lambda3.Body as MemberExpression).Member as PropertyInfo).GetMethod;

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
            Assert.That(key1.GetHashCode(), Is.Not.EqualTo(key3.GetHashCode()));
        }

        [Test]
        public void NewExpressionAreEqual_class()
        {
            LambdaExpression lambda1 = (Expression<Action>)(() => new NetworkWriter(default));
            LambdaExpression lambda2 = (Expression<Action>)(() => new NetworkWriter(default));
            LambdaExpression lambda3 = (Expression<Action>)(() => new NetworkReader());

            Assert.That(lambda1.Body, Is.InstanceOf<NewExpression>());

            object key1 = (lambda1.Body as NewExpression).Constructor;
            object key2 = (lambda2.Body as NewExpression).Constructor;
            object key3 = (lambda3.Body as NewExpression).Constructor;

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
            Assert.That(key1.GetHashCode(), Is.Not.EqualTo(key3.GetHashCode()));
        }

        [Test]
        public void NewExpressionAreEqual_sturct()
        {
            LambdaExpression lambda1 = (Expression<Action>)(() => new Vector3(default, default));
            LambdaExpression lambda2 = (Expression<Action>)(() => new Vector3(default, default));
            LambdaExpression lambda3 = (Expression<Action>)(() => new Vector2(default, default));

            Assert.That(lambda1.Body, Is.InstanceOf<NewExpression>());

            object key1 = (lambda1.Body as NewExpression).Constructor;
            object key2 = (lambda2.Body as NewExpression).Constructor;
            object key3 = (lambda3.Body as NewExpression).Constructor;

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
            Assert.That(key1.GetHashCode(), Is.Not.EqualTo(key3.GetHashCode()));
        }

        [Test]
        public void NewExpressionAreEqual_sturct_default()
        {
            LambdaExpression lambda1 = (Expression<Action>)(() => new Vector3());
            LambdaExpression lambda2 = (Expression<Action>)(() => new Vector3());
            LambdaExpression lambda3 = (Expression<Action>)(() => new Vector2());

            Assert.That(lambda1.Body, Is.InstanceOf<NewExpression>());

            var newExp1 = lambda1.Body as NewExpression;
            var newExp2 = lambda2.Body as NewExpression;
            var newExp3 = lambda3.Body as NewExpression;

            Assert.That(newExp1.Constructor, Is.Null, "Constructor is null if default struct");

            object key1 = newExp1.Type.GetConstructors().First();
            object key2 = newExp2.Type.GetConstructors().First();
            object key3 = newExp3.Type.GetConstructors().First();

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
            Assert.That(key1.GetHashCode(), Is.Not.EqualTo(key3.GetHashCode()));
        }
    }
}
