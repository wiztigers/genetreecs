using System.Collections.Generic;



public interface Node {
	/// <summary>Parent node (weakly-typed)</summary>
	Node Parent { get; }
	/// <summary>List of children nodes (weakly-typed, read-only)</summary>
	IReadOnlyList<Node> Children { get; }

	/// <summary>Adds a node as a children of this one.</summary>
	/// <typeparam name="T">Class (derived from CodeElement) of the child to-be.</typeparam>
	/// <param name="node">Child to-be.</param>
	void Add<T>(Node<T> node) where T:CodeElement;
	/// <summary>Removes a child from this node.</summary>
	/// <typeparam name="T">Class (derived from CodeElement) of the child node.</typeparam>
	/// <param name="node">Child to remove. If this is not one of this Node's current children, nothing happens.</param>
	void Remove<T>(Node<T> node) where T:CodeElement;
	/// <summary>Removes this node from its Parent's children list and set this.Parent to null.</summary>
	void Remove();

	/// <summary>Unique identifier of this Node in its tree.</summary>
	string URI { get; }
	/// <summary>Retrieves this Node or one of its (in)direct children using a node URI.</summary>
	/// <param name="uri">Node unique identifier.</param>
	/// <returns>The Node n with n.URI.EndsWith(uri), or null if there is no such Node.</returns>
	Node Get(string uri);

	void Dump(System.Text.StringBuilder str, int i);
}
public abstract class Node<T>: Node where T:CodeElement {

	public T CodeElement;
	public Node(T ce) { CodeElement = ce; }

	public Node Parent { get; private set; }
	protected List<Node> children = new List<Node>();
	/// <summary>List of children. If you want to modify it, use the Add and Remove methods.</summary>
	public IReadOnlyList<Node> Children { get { return children.AsReadOnly(); } }

	public void Add<T>(Node<T> node) where T:CodeElement {
		children.Add(node);
		node.Parent = this;
	}
	public void Remove<T>(Node<T> node) where T:CodeElement {
		children.Remove(node);
		node.Parent = null;
	}
	public void Remove() {
		if (Parent != null) Parent.Remove(this);
	}

	public virtual string ID { get { return null; } }
	public string URI {
		get {
			if (ID == null) return null;
			string puri = Parent == null?null:Parent.URI;
			if (puri == null) return ID;
			return puri+'.'+ID;
		}
	}
	/// <summary>Get this node or one of its children that has a given URI.</summary>
	/// <param name="uri">Node unique identifier to search for</param>
	/// <returns>Node n for which n.URI == uri, or null if no such Node was found</returns>
	public Node Get(string uri) {
		if (URI != null && URI.EndsWith(uri)) return this;
		foreach(var child in Children) {
			var found = child.Get(uri);
			if (found != null) return found;
		}
		return null;
	}
	public N Get<N>(string uri) where N:Node {
		var node = Get(uri);
		try { return (N)node; }
		catch(System.InvalidCastException) { return default(N); }
	}



	public override string ToString() {
		var str = new System.Text.StringBuilder();
		Dump(str, 0);
		return str.ToString();
	}
	// and what if I DON'T want to put Dump(..) inside the Node interface? T_T
	// Dump(..) should be private !!
	public void Dump(System.Text.StringBuilder str, int i) {
		for (int c=0; c<i; c++) str.Append("  ");
		if (CodeElement == null) str.AppendLine("?");
		else str.AppendLine(CodeElement.ToString());
		foreach(var child in Children) child.Dump(str, i+1);
	}
}
/// <summary>Root Node of a tree of Nodes.</summary>
public class Root: Node<CodeElement> {
	public Root(): base(null) { }
}





/// <summary>A <see cref="Node"/> who can type its parent more strongly should inherit from this.</summary>
/// <typeparam name="C">Class (derived from <see cref="Node{T}"/>) of the parent node.</typeparam>
public interface Child<P> where P:Node { }
/// <summary>Extension method to get a more strongly-typed parent than just Node.</summary>
public static class ChildExtension {
	/// <summary>Returns this node's parent in as strongly-typed.</summary>
	/// <typeparam name="P">Class (derived from <see cref="Node{T}"/>) of the parent.</typeparam>
	/// <param name="child">We want this <see cref="Node"/>'s parent.</param>
	/// <returns>This <see cref="Node"/>'s parent, but strongly-typed.</returns>
	public static P StrongParent<P>(this Child<P> child) where P:Node {
		var node = child as Node;
		if (node == null) throw new System.ArgumentException("Child must be a Node.");
		return (P)node.Parent;
    }
}

/// <summary>A <see cref="Node"/> who can type its children more strongly should inherit from this.</summary>
/// <typeparam name="C">Class (derived from <see cref="Node{T}"/>) of the children nodes.</typeparam>
public interface Parent<C> where C:Node { }
/// <summary>Extension method to get children more strongly-typed than just Node.</summary>
public static class ParentExtension {
	/// <summary>
	/// Returns a read-only list of strongly-typed children of a <see cref="Node"/>.
	/// The children are more strongly-typed than the ones in the Node.Children property.
	/// The list is read-only because the returned list is a copy of the Node.Children list property ;
	/// thus, writing node.StrongChildren().Add(child) will be a compilation error.
	/// Strongly-typed children are to be iterated on, but to modify a Node's children list you have
	/// to use the (weakly-typed) Node.Children property.
	/// </summary>
	/// <typeparam name="C">Class (derived from <see cref="Node{T}"/>) of the children.</typeparam>
	/// <param name="parent">We want this <see cref="Node"/>'s children.</param>
	/// <returns>Strongly-typed list of a <see cref="Node"/>'s children.</returns>
	public static IReadOnlyList<C> StrongChildren<C>(this Parent<C> parent) where C:Node {
		var node = parent as Node;
		if (node == null) throw new System.ArgumentException("Parent must be a Node.");
		//TODO? maybe use ConvertAll or Cast from LINQ, but only
		// if the performance is better or if it avoids a copy.
		var result = new List<C>();
		foreach(var child in node.Children) result.Add((C)child);
        return result.AsReadOnly();
    }
}





class Program {
	Root Root { get; set; }
	Node Current { get; set; }

	public Program() {
		this.Root = new Root();
		this.Current = this.Root;
	}

	public void Enter<T>(Node<T> node) where T:CodeElement {
		Current.Add(node);
		Current = node;
	}
	public void Exit() {
		Current = Current.Parent;
	}

	static void Main(string[] args) {
		var program = new Program();
	program.Enter(new DataDivision(new DataDivisionHeader()));
		program.Enter(new WorkingStorageSection(new WorkingStorageSectionHeader()));
			program.Enter(new TypeDescription(new TypeDescriptionEntry { Level = 1, Name = "POINT2D" }));
				program.Enter(new DataDescription(new DataDescriptionEntry { Level =  2, Name = "x" })); program.Exit();
				program.Enter(new DataDescription(new DataDescriptionEntry { Level = 2, Name = "y" })); program.Exit();
			program.Exit();
		program.Exit();
		program.Enter(new LinkageSection(new LinkageSectionHeader()));
			program.Enter(new DataDescription(new DataDescriptionEntry { Level =  1, Name = "x" }));
				program.Enter(new DataDescription(new DataDescriptionEntry { Level =  5, Name = "a" }));
					program.Enter(new DataDescription(new DataDescriptionEntry { Level = 10, Name = "b" })); program.Exit();
				program.Exit();
				program.Enter(new DataDescription(new DataDescriptionEntry { Level = 5, Name = "c" })); program.Exit();
			program.Exit();
			program.Enter(new DataDescription(new DataDescriptionEntry { Level = 1, Name = "y" })); program.Exit();
			program.Enter(new DataDescription(new DataDescriptionEntry { Level = 1, Name = "z" })); program.Exit();
			program.Enter(new DataCondition(new DataConditionEntry { Level = 88, Name = "b" })); program.Exit();
			program.Enter(new DataCondition(new DataConditionEntry { Level = 88, Name = "c" })); program.Exit();
			program.Enter(new TypeDescription(new TypeDescriptionEntry { Level = 1, Name = "POINT3D" }));
				program.Enter(new DataDescription(new DataDescriptionEntry { Level =  2, Name = "x" })); program.Exit();
				program.Enter(new DataDescription(new DataDescriptionEntry { Level = 2, Name = "y" })); program.Exit();
				program.Enter(new DataDescription(new DataDescriptionEntry { Level =  2, Name = "z" })); program.Exit();
			program.Exit();
		program.Exit();
	program.Exit();

		System.Console.WriteLine(program.Root.ToString());
		System.Console.WriteLine(program.Root.Get("x").ToString());
		System.Console.WriteLine(program.Root.Get("POINT3D.x").ToString());

		var typedef = program.Root.Get<TypeDescription>("POINT3D");
		System.Console.WriteLine("typedef:"+typedef.URI+"("+typedef.IsStrong+") < "+typedef.StrongParent().URI+","+typedef.StrongParent().IsShared+" #"+typedef.Children.Count);
		foreach(var child in typedef.StrongChildren()) System.Console.WriteLine("child: "+child.URI+"("+child.Name+")");
		typedef = program.Root.Get<TypeDescription>("POINT2D");
		System.Console.WriteLine("typedef:"+typedef.URI+"("+typedef.IsStrong+") < "+typedef.StrongParent().URI+","+typedef.StrongParent().IsShared+" #"+typedef.Children.Count);
		foreach(var child in typedef.StrongChildren()) System.Console.WriteLine("child: "+child.URI+"("+child.Name+")");

		var section = program.Root.Get<DataSection>("linkage");
		bool shared = section.IsShared;
		string firstchild = section.StrongChildren()[0].Name;
		var toto = new DataDescription(new DataDescriptionEntry { Level = 1, Name = "toto"});
		System.Console.WriteLine("before ADD: "+section.Children.Count+" vs "+section.StrongChildren().Count);
		// this is a compile-time error
		// section.StrongChildren().Add(toto);
		// so use the following instead:
		section.Add(toto);
		System.Console.WriteLine(" after ADD: "+section.Children.Count+" vs "+section.StrongChildren().Count);
		toto.Remove();
		System.Console.WriteLine(" after REMOVE: "+section.Children.Count+" vs "+section.StrongChildren().Count);
		section = program.Root.Get<DataSection>("working");
		System.Console.WriteLine("before MOVE: "+section.Children.Count+" vs "+section.StrongChildren().Count);
		section.Add(toto);
		System.Console.WriteLine(" after MOVE: "+section.Children.Count+" vs "+section.StrongChildren().Count);

		System.Console.ReadLine();
	}
}

