﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Unity.TestSupport;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif __IOS__
using NUnit.Framework;
using TestClassAttribute = NUnit.Framework.TestFixtureAttribute;
using TestInitializeAttribute = NUnit.Framework.SetUpAttribute;
using TestMethodAttribute = NUnit.Framework.TestAttribute;
#else
using Xunit;
#endif

namespace ObjectBuilder2.Tests
{
     
    public class BuildPlanStrategyFixture
    {
        [Fact]
        public void StrategyGetsBuildPlanFromPolicySet()
        {
            MockBuilderContext context = new MockBuilderContext();
            context.Strategies.Add(new BuildPlanStrategy());
            object instance = new object();
            ReturnInstanceBuildPlan plan = new ReturnInstanceBuildPlan(instance);

            context.Policies.Set<IBuildPlanPolicy>(plan, new NamedTypeBuildKey<object>());

            object result = context.ExecuteBuildUp(new NamedTypeBuildKey<object>(), null);

            Assert.True(plan.BuildUpCalled);
            Assert.Same(instance, result);
        }

        [Fact]
        public void StrategyCreatesBuildPlanWhenItDoesntExist()
        {
            MockBuilderContext context = new MockBuilderContext();
            context.Strategies.Add(new BuildPlanStrategy());
            MockBuildPlanCreatorPolicy policy = new MockBuildPlanCreatorPolicy();
            context.Policies.SetDefault<IBuildPlanCreatorPolicy>(policy);

            object result = context.ExecuteBuildUp(new NamedTypeBuildKey<object>(), null);

            Assert.NotNull(result);
            Assert.True(policy.PolicyWasCreated);

            IBuildPlanPolicy plan = context.Policies.Get<IBuildPlanPolicy>(new NamedTypeBuildKey(typeof(object)));
            Assert.NotNull(plan);
        }
    }

    internal class MockBuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
    {
        private bool policyWasCreated = false;

        public IBuildPlanPolicy CreatePlan(IBuilderContext context, NamedTypeBuildKey buildKey)
        {
            policyWasCreated = true;
            return new ReturnInstanceBuildPlan(new object());
        }

        public bool PolicyWasCreated
        {
            get { return policyWasCreated; }
            set { policyWasCreated = value; }
        }
    }

    internal class ReturnInstanceBuildPlan : IBuildPlanPolicy
    {
        private object instance;
        private bool buildUpCalled;

        public ReturnInstanceBuildPlan(object instance)
        {
            this.instance = instance;
            this.buildUpCalled = false;
        }

        public void BuildUp(IBuilderContext context)
        {
            buildUpCalled = true;
            context.Existing = instance;
        }

        public bool BuildUpCalled
        {
            get { return buildUpCalled; }
        }

        public object Instance
        {
            get { return instance; }
        }
    }
}
