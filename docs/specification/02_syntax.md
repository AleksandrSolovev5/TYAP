# Грамматика языка clvr

## Приоритеты операторов

# Таблица приоритетов операторов языка clvr

| Приоритет  | Операторы                        | Тип операции   | Ассоциативность |
| ---------- | -------------------------------- | -------------- | --------------- |
| 7 (высший) | `+`, `-`, `!`                    | Унарная        | Правая          |
| 6          | `*`, `/`, `%`                    | Арифметическая | Левая           |
| 5          | `+`, `-`                         | Арифметическая | Левая           |
| 4          | `<`, `>`, `==`, `!=`, `<=`, `>=` | Сравнение      | Левая           |
| 3          | `&&`                             | Логическое И   | Левая           |
| 2          | `\|\|`                           | Логическое ИЛИ | Левая           |
| 1 (низший) | `=`                              | Присваивание   | Правая          |

## EBNF - грамматика

```
program = statement, { ";", statement }, [ ";" ] ;
type = "int" | "string" | "float" | "bool" ;

statement =
    variable_declaration
  | constant_definition
  | assignment
  | input_statement
  | output_statement
  | compound_statement
  | if_statement ;

variable_declaration = type, identifier, [ "=", expression ], { ",", identifier, [ "=", expression ] } ;
constant_definition = "const", type, identifier, "=", expression ;
assignment = identifier, "=", expression ;

input_statement = "input", "(", identifier, { ",", identifier }, ")" ;
output_statement = "output", "(", [ expression_list ], ")" ;
expression_list = expression, { ",", expression } ;

compound_statement = "[", statement, { ";", statement }, [ ";" ], "]" ;

if_statement = "if", "(", expression, ")", statement, [ "else", statement ] ;


expression = logical_or_expression ;
logical_or_expression = logical_and_expression, { "||", logical_and_expression } ;
logical_and_expression = comparison_expression, { "&&", comparison_expression } ;
comparison_expression = additive_expression, { ("<" | ">" | "==" | "!=" | "<=" | ">="), additive_expression } ;
additive_expression = multiplicative_expression, { ( "+" | "-" ), multiplicative_expression } ;
multiplicative_expression = unary_expression, { ( "*" | "/" | "%" ), unary_expression } ;
unary_expression = ( "+" | "-" | "!" ), unary_expression | postfix_expression ;
postfix_expression = primary_expression { "[" expression "]" } ;
primary_expression = identifier | literal | "(", expression, ")" | len_call ;
len_call = "len", "(", expression, ")" ;
```
