using System;
using System.Collections;
using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AvoidMemberWithManyStatementsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidMemberWithManyStatementsAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_contains_eight_empty_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            ; ;
                            ; ;
                            ; ;
                            ; ;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_empty_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            ; ;
                            ; ;
                            ; ;

                            // Block scopes are not counted
                            {
                                {
                                    ;
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_declaration_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            int statement1 = 1;
                            int statement2 = 2;
                            int statement3 = 3;
                            int statement4 = 4;

                            int statement5 = 5;
                            int statement6 = 6;
                            int statement7 = 7;
                            int statement8 = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_declaration_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            int statement1 = 1;
                            int statement2 = 2;
                            int statement3 = 3;
                            int statement4 = 4;

                            int statement5 = 5;
                            int statement6 = 6;
                            int statement7 = 7, other = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_expression_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private int i;

                        void [|M|]()
                        {
                            i = 1;
                            i++;
                            i += 3;
                            i--;

                            i += (true ? 5 : 0);
                            i *= 6;
                            i--;
                            i += 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_expression_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private int i;

                        void M()
                        {
                            i = 1;
                            i++;
                            i += 3;
                            i--;

                            i += (true ? 5 : 0);
                            i *= 6;
                            i--;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_for_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            for (int a = 0; a < 1000; a++)
                                for (int b = 0; b < 1000; b++)
                                    for (int c = 0; c < 1000; c++)
                                        for (int d = 0; d < 1000; d++)
                                        {
                                            for (int e = 0; e < 1000; e++)
                                                for (int f = 0; f < 1000; f++)
                                                    for (int g = 0; g < 1000; g++)
                                                        for (int h = 0; h < 1000; h++)
                                                        {
                                                        }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_for_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            for (int a = 0; a < 1000; a++)
                                for (int b = 0; b < 1000; b++)
                                    for (int c = 0; c < 1000; c++)
                                        for (int d = 0; d < 1000; d++)
                                        {
                                            for (int e = 0; e < 1000; e++)
                                                for (int f = 0; f < 1000; f++)
                                                    for (int g = 0; g < 1000; g++)
                                                    {
                                                    }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_foreach_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            foreach (var level1 in new string[0])
                                foreach (var level2 in new string[0])
                                    foreach (var level3 in new string[0])
                                        foreach (var level4 in new string[0])
                                        {
                                            foreach (var level5 in new string[0])
                                                foreach (var level6 in new string[0])
                                                    foreach (var level7 in new string[0])
                                                        foreach (var level8 in new string[0])
                                                        {
                                                        }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_foreach_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            foreach (var level1 in new string[0])
                                foreach (var level2 in new string[0])
                                    foreach (var level3 in new string[0])
                                        foreach (var level4 in new string[0])
                                        {
                                            foreach (var level5 in new string[0])
                                                foreach (var level6 in new string[0])
                                                    foreach (var level7 in new string[0])
                                                    {
                                                    }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_while_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            while (true)
                                while (true)
                                    while (true)
                                        while (true)

                                            while (true)
                                                while (true)
                                                    while (true)
                                                        while (true)
                                                        {
                                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_while_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            while (true)
                                while (true)
                                    while (true)
                                        while (true)

                                            while (true)
                                                while (true)
                                                    while (true)
                                                    {
                                                    }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_do_while_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                        }
                                        while (true);
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);

                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                        }
                                        while (true);
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_do_while_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                        }
                                        while (true);
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);

                            do
                            {
                                do
                                {
                                    do
                                    {
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_if_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                    if (true)
                                    {
                                        if (true)
                                        {
                                        }
                                        else
                                        {
                                            if (true)
                                            {
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            else
                            {
                            }

                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }

                            if (true)
                            {
                            }
                            else
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_if_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                    if (true)
                                    {
                                        if (true)
                                        {
                                        }
                                        else
                                        {
                                            if (true ? false : true)
                                            {
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            else
                            {
                            }

                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_local_function_declarations_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            void L1()
                            {
                            }

                            void L2()
                            {
                            }

                            void L3()
                            {
                            }

                            void L4()
                            {
                            }

                            void L5()
                            {
                            }

                            void L6()
                            {
                            }

                            void L7()
                            {
                            }

                            void L8()
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_local_function_invocations_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            void L()
                            {
                            }

                            L();
                            L();
                            L();
                            L();

                            L();
                            L();
                            L();
                            L();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_local_function_invocations_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            void L()
                            {
                            }

                            L();
                            L();
                            L();
                            L();

                            L();
                            L();
                            L();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_switch_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](int i)
                        {
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }

                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(int)' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_switch_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int i)
                        {
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }

                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_statements_in_switch_case_blocks_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](int i)
                        {
                            switch (i)
                            {
                                case 1:
                                {
                                    ; ; ;
                                    break;
                                }
                                default:
                                {
                                    ; ;
                                    break;
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(int)' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_statements_in_switch_case_blocks_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int i)
                        {
                            switch (i)
                            {
                                case 1:
                                {
                                    ; ;
                                    break;
                                }
                                default:
                                {
                                    ; ;
                                    break;
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_throw_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](string s)
                        {
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();

                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_throw_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(string s)
                        {
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();

                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_try_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    try
                                    {
                                    }
                                    finally
                                    {
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }

                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    try
                                    {
                                    }
                                    finally
                                    {
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_try_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    try
                                    {
                                    }
                                    finally
                                    {
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }

                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_using_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private IDisposable d;

                        void [|M|]()
                        {
                            using (d)
                                using (d)
                                    using (d)
                                        using (d)
                                        {
                                        }

                            using (d)
                                using (d)
                                    using (d)
                                        using (d)
                                        {
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_using_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private IDisposable d;

                        void M()
                        {
                            using (d)
                                using (d)
                                    using (d)
                                        using (d)
                                        {
                                        }

                            using (d)
                                using (d)
                                    using (var x = d)
                                    {
                                    }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_property_getter_contains_eight_return_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        public int [|P|]
                        {
                            get
                            {
                                return 1;
                                return 2;
                                return 3;
                                return 4;

                                return 5;
                                return 6;
                                return 7;
                                return 8;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'C.P' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_return_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        public int P
                        {
                            get
                            {
                                return 1;
                                return 2;
                                return 3;
                                return 4;

                                return 5;
                                return 6;
                                return 7;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_yield_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable [|M|]()
                        {
                            yield break;
                            yield return new object();
                            yield break;
                            yield return new object();

                            yield break;
                            yield return new object();
                            yield break;
                            yield return new object();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_yield_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M()
                        {
                            yield break;
                            yield return new object();
                            yield break;
                            yield return new object();

                            yield break;
                            yield return new object();
                            yield break;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_lock_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private static readonly object guard = new object();

                        void [|M|]()
                        {
                            lock (guard)
                            {
                                lock (guard)
                                {
                                    lock (guard)
                                    {
                                        lock (guard)
                                        {

                                            lock (guard)
                                            {
                                                lock (guard)
                                                {
                                                    lock (guard)
                                                    {
                                                        lock (guard)
                                                        {
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_lock_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private static readonly object guard = new object();

                        void M()
                        {
                            lock (guard)
                            {
                                lock (guard)
                                {
                                    lock (guard)
                                    {
                                        lock (guard)
                                        {

                                            lock (guard)
                                            {
                                                lock (guard)
                                                {
                                                    lock (guard)
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_fixed_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        unsafe void [|M|]()
                        {
                            fixed (char* p1 = Environment.MachineName, p2 = Environment.MachineName)
                                fixed (char* p3 = Environment.MachineName, p4 = Environment.MachineName)
                                    fixed (char* p5 = Environment.MachineName, p6 = Environment.MachineName)
                                        fixed (char* p7 = Environment.MachineName, p8 = Environment.MachineName)
                                        {
                                        }

                            fixed (char* p9 = Environment.MachineName, p10 = Environment.MachineName)
                            {
                                fixed (char* p11 = Environment.MachineName, p12 = Environment.MachineName)
                                {
                                    fixed (char* p13 = Environment.MachineName, p14 = Environment.MachineName)
                                    {
                                        fixed (char* p15 = Environment.MachineName, p16 = Environment.MachineName)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_fixed_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        unsafe void M()
                        {
                            fixed (char* p1 = Environment.MachineName, p2 = Environment.MachineName)
                                fixed (char* p3 = Environment.MachineName, p4 = Environment.MachineName)
                                    fixed (char* p5 = Environment.MachineName, p6 = Environment.MachineName)
                                        fixed (char* p7 = Environment.MachineName, p8 = Environment.MachineName)
                                        {
                                        }

                            fixed (char* p9 = Environment.MachineName, p10 = Environment.MachineName)
                            {
                                fixed (char* p11 = Environment.MachineName, p12 = Environment.MachineName)
                                {
                                    fixed (char* p13 = Environment.MachineName, p14 = Environment.MachineName)
                                    {
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_unsafe_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_unsafe_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_eight_checked_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            checked
                            {
                                unchecked
                                {
                                }
                            }

                            unchecked
                            {
                                checked
                                {
                                }
                            }

                            checked
                            {
                            }

                            unchecked
                            {
                            }

                            checked
                            {
                            }

                            unchecked
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_seven_checked_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            checked
                            {
                                unchecked
                                {
                                }
                            }

                            unchecked
                            {
                                checked
                                {
                                }
                            }

                            checked
                            {
                            }

                            unchecked
                            {
                            }

                            checked
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_labels_and_eight_goto_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            Label1:
                                goto Label2;
                            Label2:
                                goto Label3;
                            Label3:
                                goto Label4;
                            Label4:
                                goto Label5;

                            Label5:
                                goto Label6;
                            Label6:
                                goto Label7;
                            Label7:
                                goto Label8;
                            Label8:
                                goto Label1;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_labels_and_seven_goto_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            Label1:
                                goto Label2;
                            Label2:
                                goto Label3;
                            Label3:
                                goto Label4;
                            Label4:
                                goto Label5;

                            Label5:
                                goto Label6;
                            Label6:
                                goto Label7;
                            Label7:
                                goto Label1;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_six_statements_and_an_invocation_with_lambda_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class C
                    {
                        public void M()
                        {
                            ; ; ; ; ; ;

                            Other(() => Empty());
                        }

                        protected abstract void Other(Action action);
                        protected abstract void Empty();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_six_statements_and_an_invocation_with_lambda_statement_block_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class C
                    {
                        public void [|M|]()
                        {
                            ; ; ; ; ; ;

                            Other(() =>
                            {
                                Empty();
                            });
                        }

                        protected abstract void Other(Action action);
                        protected abstract void Empty();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_method_contains_mixed_set_of_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IDisposable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IDisposable AcquireResource() => throw null; 
                        int field;

                        void [|M1|]()
                        {
                            using (var resource = AcquireResource())                    // 1
                            {
                                int i = true ? 1 : throw new NotSupportedException();   // 2

                                try                                                     // 3
                                {
                                    int j = checked(i++);                               // 4
                                    Action action = () => throw null;                   // 5
                                }
                                catch (Exception ex)
                                {
                                    return;                                             // 6
                                    throw;                                              // 7
                                }
                                finally
                                {
                                    ;                                                   // 8
                                }
                            }
                        }

                        void [|M2|](int i)
                        {
                            switch (i)                                      // 1
                            {
                                case 1:
                                case 2:
                                case 3:
                                    goto case 4;                            // 2
                                case 4:
                                    goto default;                           // 3
                                default:
                                {
                                    throw null;                             // 4
                                }
                            }

                            unsafe                                          // 5
                            {
                                fixed (int* p = &field)                     // 6
                                {
                                    var x = stackalloc int[] { 1, 2, 3 };   // 7
                                    int y = *x;                             // 8
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M1()' contains 8 statements, which exceeds the maximum of 7 statements.",
                "Method 'C.M2(int)' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        [Fact]
        internal void When_async_method_contains_mixed_set_of_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        async Task [|M|](bool b)
                        {
                            if (b)                      // 1
                            {
                                throw null;             // 2
                            }

                            await Task.Yield();         // 3

                            if (!b)                     // 4
                            {
                                await Task.Yield();     // 5
                                await Task.Yield();     // 6
                            }
                            else
                                throw null;             // 7

                            return;                     // 8
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(bool)' contains 8 statements, which exceeds the maximum of 7 statements.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidMemberWithManyStatementsAnalyzer();
        }
    }
}
