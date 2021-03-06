﻿using NUnit.Framework;

namespace JUST.UnitTests.Arrays
{
    [TestFixture, Category("Loops")]
    public class LoopingTests
    {
        [Test]
        public void CurrentValue()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_value\": \"#currentvalue()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value\":1},{\"current_value\":2},{\"current_value\":3},{\"current_value\":4},{\"current_value\":5}]}", result);
        }

        [Test]
        public void CurrentIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_index\": \"#currentindex()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_index\":0},{\"current_index\":1},{\"current_index\":2},{\"current_index\":3},{\"current_index\":4}]}", result);
        }

        [Test]
        public void LastIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_index\": \"#lastindex()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4}]}", result);
        }

        [Test]
        public void LastValue()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_value\": \"#lastvalue()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5}]}", result);
        }

        [Test]
        public void CurrentValueAtPath()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value_at_path\":\"Norway\"},{\"current_value_at_path\":\"UK\"},{\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void LastValueAtPath()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"last_value_at_path\": \"#lastvalueatpath($.country.language)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"}]}", result);
        }

        [Test]
        public void NestedLooping()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loopwithincontext($.Details)\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void FunctionAsLoopArgument()
        {
            const string transformer = "{ \"hello\": { \"#loop(#xconcat($.NestedLoop.,Organization,.Employee))\": { \"Details\": { \"#loopwithincontext(#concat($.,Details))\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void PrimitiveTypeArrayResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": \"#currentvalueatpath($.name)\" } }";

            var result = JsonTransformer.Transform(transformer, input, new JUSTContext {EvaluationMode = EvaluationMode.Strict});

            Assert.AreEqual("{\"result\":[\"Person 1\",\"Person 3\"]}", result);
        }

        [Test]
        public void ObjectTypeArrayResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": \"#currentvalue()\" } }";

            var result = JsonTransformer.Transform(transformer, input, new JUSTContext { EvaluationMode = EvaluationMode.Strict });

            Assert.AreEqual("{\"result\":[{\"id\":1,\"name\":\"Person 1\",\"gender\":\"M\"},{\"id\":3,\"name\":\"Person 3\",\"gender\":\"M\"}]}", result);
        }

        [Test]
        public void LoopOverProperties()
        {
            var input = "{ \"animals\": { \"cat\": { \"legs\": 4, \"sound\": \"meow\" }, \"dog\": { \"legs\": 4, \"sound\": \"woof\" } }, \"spell_numbers\": { \"3\": \"three\", \"2\": \"two\", \"1\": \"one\" } }";
            var transformer = "{ \"sounds\": { \"#loop($.animals)\": { \"#eval(#currentproperty())\": \"#currentvalueatpath($..sound)\" } }, \"number_index\": { \"#loop($.spell_numbers)\": { \"#eval(#currentindex())\": \"#currentvalueatpath(#concat($.,#currentproperty()))\" } }}";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);

            Assert.AreEqual("{\"sounds\":{\"cat\":\"meow\",\"dog\":\"woof\"},\"number_index\":{\"0\":\"three\",\"1\":\"two\",\"2\":\"one\"}}", result);
        }

        [Test]
        public void EmptyArrayLooping()
        {
            const string input = "{ \"Sex\": \"Female\", \"Colors\": [ ]}";
            const string transformer = "{ \"Colors\": { \"#loop($.Colors)\": \"#currentvalue()\" } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);

            Assert.AreEqual("{\"Colors\":[]}", result);
        }

        [Test]
        public void NullLooping()
        {
            var input = "{ \"spell_numbers\": null }";
            var transformer = "{ \"number_index\": { \"#loop($.spell_numbers)\": { \"#eval(#currentindex())\": \"#currentvalueatpath(#concat($.,#currentproperty()))\" } } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);
            Assert.AreEqual("{\"number_index\":null}", result);
        }

        [Test]
        public void EmptyPropertiesLooping()
        {
            var input = "{ \"animals\": { } }";
            var transformer = "{ \"sounds\": { \"#loop($.animals)\": { \"#eval(#currentproperty())\": \"#currentvalueatpath($..sound)\" } } }";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);
            Assert.AreEqual("{\"sounds\":{}}", result);
        }
        
        [Test]
        public void SingleResultFilter()
        {
            var input = "{\"array\":[{\"resource\":\"Location\",\"number\":\"3\" },{\"resource\":\"Organization\",\"number\":\"10\"}] }";
            var transformer = "{\"result\":{\"#loop($.array[?(@resource=='Location')])\":{\"existsLocation\":true}}}";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);
            Assert.AreEqual("{\"result\":[{\"existsLocation\":true}]}", result);
        }

        [Test]
        public void SingleIndexReference()
        {
            var input = "{\"array\":[{\"resource\":\"Location\",\"number\":\"3\" },{\"resource\":\"Organization\",\"number\":\"10\"}] }";
            var transformer = "{\"result\": {\"#loop($.array[1])\": {\"number\":\"#currentvalueatpath($.number)\"} }}";
            var context = new JUSTContext
            {
                EvaluationMode = EvaluationMode.Strict
            };
            var result = JsonTransformer.Transform(transformer, input, context);
            Assert.AreEqual("{\"result\":[{\"number\":\"10\"}]}", result);
        }

        [Test]
        public void BulkFunctions()
        {
            const string input = "{\"score_PCS\": [{\"data\": \"2020-04-08T10:20:21.335+00:00\",\"score\": [{\"score_type\": \"pcs_tot\",\"score_value\": 0.5},{\"score_type\": \"pcs_help\",\"score_value\": 0.46},{\"score_type\": \"pcs_rum\",\"score_value\": 0.5},{\"score_type\": \"pcs_mag\",\"score_value\": 0.63}]},{\"data\": \"2020-04-09T10:22:03.267+00:00\",\"score\": [{\"score_type\": \"pcs_tot\",\"score_value\": 0.38},{\"score_type\": \"pcs_help\",\"score_value\": 0.42},{\"score_type\": \"pcs_rum\",\"score_value\": 0.35},{\"score_type\": \"pcs_mag\",\"score_value\": 0.38}]},{\"data\": \"2020-04-09T10:23:05.748+00:00\",\"score\": [{\"score_type\": \"pcs_tot\",\"score_value\": 0.44},{\"score_type\": \"pcs_help\",\"score_value\": 0.38},{\"score_type\": \"pcs_rum\",\"score_value\": 0.5},{\"score_type\": \"pcs_mag\",\"score_value\": 0.5}]}]}";
            const string transformer = "{ \"score_pcs_tot\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_tot')])\" ], \"score_data\": \"#currentvalueatpath($.data)\" } }, \"score_pcs_help\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_help')])\", \"#replace($.score_type, #currentvalueatpath($.score[?(@.score_type=='pcs_rum')]))\" ], \"score_data\": \"#currentvalueatpath($.data)\" } }, \"score_pcs_rum\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_rum')])\", \"#replace($.score_type, #currentvalueatpath($.score[?(@.score_type=='pcs_help')].score_type))\" ], \"score_data\": \"#currentvalueatpath($.data)\" } }, \"score_pcs_mag\": { \"#loop($.score_PCS)\": { \"#\": [ \"#copy($.score[?(@.score_type=='pcs_mag')])\", \"#delete($.score_type)\" ], \"score_data\": \"#currentvalueatpath($.data)\" } } }";

            var result = JsonTransformer.Transform(transformer, input);

            Assert.AreEqual("{\"score_pcs_tot\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_type\":\"pcs_tot\",\"score_value\":0.5},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_type\":\"pcs_tot\",\"score_value\":0.38},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_type\":\"pcs_tot\",\"score_value\":0.44}],\"score_pcs_help\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_type\":{\"score_type\":\"pcs_rum\",\"score_value\":0.5},\"score_value\":0.46},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_type\":{\"score_type\":\"pcs_rum\",\"score_value\":0.35},\"score_value\":0.42},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_type\":{\"score_type\":\"pcs_rum\",\"score_value\":0.5},\"score_value\":0.38}],\"score_pcs_rum\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_type\":\"pcs_help\",\"score_value\":0.5},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_type\":\"pcs_help\",\"score_value\":0.35},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_type\":\"pcs_help\",\"score_value\":0.5}],\"score_pcs_mag\":[{\"score_data\":\"2020-04-08T10:20:21.335+00:00\",\"score_value\":0.63},{\"score_data\":\"2020-04-09T10:22:03.267+00:00\",\"score_value\":0.38},{\"score_data\":\"2020-04-09T10:23:05.748+00:00\",\"score_value\":0.5}]}", result);
        }
    }
}
