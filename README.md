A simple file compiler to compile different files and replace text in different files.

## Usage:
|Input/file1.txt|
|-|
|
```
${greet world}
${greet "all the people of the world"}
${greet person1 person2}
```

|Input/greet.txt|
|-|
|
```
Hello
$[0]
$[1]
```

Run: `FileCompiler Input -o Output`

The following files will appear:

|Output/file1.txt|
|-|
|
```
Hello
world

Hello
all the people of the world

Hello
person1
person2
```

|Output/greet.txt|
|-|
|
```
Hello
$[0]
$[1]
```
