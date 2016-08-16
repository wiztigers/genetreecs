C# is a strongly typed language, so a node must be strongly typed, because using an all-purpose Node class
which just parent/child operations to modify it, a string URI to retrieve it, and a map of properties 
to analyze its CodeElementData has been deemed not sufficient.

But if I have to do that, I want at least an inheritance tree for these strongly-typed nodes, 
because I don't want to duplicate code, C# doesn't allow multiple inheritance, interfaces cannot provide 
implementation, and code would be a mess if I put everything in partial class or extension methods.
So we'll keep a `Node` at the top as with my first implementation, but it will become abstract.

If I don't want to clutter the sourcecode with incessant casts each time I want to access the `CodeElement`
data of a node, it seems I can use generics. So each node would be "nicely" genericized with its data type,
thus typing it strongly.

However, we have many cases when a node has to be manipulated independently from its type: a visitor 
traversing the `Node` tree for example, or, another example (and exceptions to the rule put aside), 
even a strongly-typed node cannot strongly-type its children.

But C# does NOT support using a list of genericized objects without knowing their generic type（TへTメ).
If you know Java, you're can think of writing `List<Node<? extends CodeElement>>`, but there is no such thing in C#.
So, what? We have to use a `Node` interface to hide the fact we have `Node<T>`
objects, for these cases when we want to iterate over nodes.

And boom, we loose our holy strongly typed nodes.
Thus, I had to find a way to RE-type strongly children of a node, because neither casting from 
`Node` nor from a `Node<CodeElement>` to a `Node<T>`> (with T being 
a subclass of `CodeElement`) will work.

However, return type covariance and is not supported in C# or .NET. (¬､¬)
- we CANNOT override a property getter return type in a derived class because, you know, it would
  give compile errors if one day I add a setter, and it is so confusing C# forbids it rather than
  force C# developpers to think.
- we CANNOT change the return type of a GetField() getter method, because this would
  require casting (ie.: `return (T)x;`) which fails at compile time, or require unboxing/boxing
  (ie.: return `(T)(object)x;`) which manages the feat to be costly, ugly AND fails at runtime.

So, we use a custom method that will do the work the language cannot figure how to do and cast
our properties ourselves. However, as C# doesn't allow multiple inheritance (；¬＿¬), and as 
`Node<T>`'s a hierarchy tree is already cluttered enough, we must use an interface.
And as C# doesn't allow interface to provide implementation for their methods (*￣m￣), we have
to use a methods extension, which is a PITA but at least does the job.



(o_ _)o
