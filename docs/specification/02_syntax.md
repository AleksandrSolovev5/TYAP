# Грамматика языка clvr

## Приоритеты операторов 

# Таблица приоритетов операторов языка clvr

| Приоритет  | Операторы                        | Тип операции   | Ассоциативность |
|------------|----------------------------------|----------------|-----------------|
| 5 (высший) | `+`, `-`                         | Унарная        | Правая          |
| 4          | `*`, `/`, `%`                    | Арифметическая | Левая           |
| 3          | `+`, `-`                         | Арифметическая | Левая           |
| 2          | `<`, `>`, `<=`, `>=`, `==`, `!=` | Сравнение      | Неассоциативная |
| 1 (низший) | `=`                              | Присваивание   | Правая          |

## EBNF - грамматика
````
program = statement, { ";", statement }, [ ";" ] ;
type = "int" | "string" | "float";

statement =
      variable_declaration
    | constant_definition
    | assignment
    | input_statement
    | output_statement
    | compound_statement ;

variable_declaration = type, identifier, [ "=", expression ], { ",", type, identifier, [ "=", expression ] } ;
constant_definition ="const", type, identifier, "=", expression ;
assignment = identifier, "=", expression ;


input_statement = "input", "(", identifier, { ",", identifier }, ")" ;
output_statement = "output", "(", [ expression_list ], ")" ;
expression_list = expression, { ",", expression } ;

statement_or_block = statement | compound_statement ;
compound_statement = "[", statement, { ";", statement }, [ ";" ], "]" ;

expression = equality_expression ;
equality_expression = relational_expression, { ( "==" | "!=" ), relational_expression } ;
relational_expression = additive_expression, { ( "<" | ">" | "<=" | ">=" ), additive_expression } ;
additive_expression = multiplicative_expression, { ( "+" | "-" ), multiplicative_expression } ;
multiplicative_expression = unary_expression, { ( "*" | "/" | "%" ), unary_expression } ;
unary_expression = ( "+" | "-" ), unary_expression | primary_expression ;
primary_expression = identifier | literal | "(", expression, ")" ;
````
