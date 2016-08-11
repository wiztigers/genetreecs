using System.Collections.Generic;


public class CodeElement {
	public override string ToString() { return this.GetType().Name; }
}
public class DataDivisionHeader: CodeElement { }
public class WorkingStorageSectionHeader: CodeElement { }
public class LinkageSectionHeader: CodeElement { }
public abstract class DataDefinitionEntry: CodeElement {
	public string Name { get; set; }
	public int Level { get; set; }
	public DataDefinitionEntry() { Name = "?"; Level =  1; }
	public override string ToString() { return string.Format("{0:00}",Level)+' '+Name; }
}
public class DataDescriptionEntry: DataDefinitionEntry {
	public string Picture { get; set; }
}
public class DataConditionEntry: DataDefinitionEntry { }
public class TypeDescriptionEntry: DataDefinitionEntry {
	public int Size { get; set; }
}



public interface Node {
	void Add<T>(Node<T> node) where T:CodeElement;
	Node Parent { get; }
	IList<Node> Children { get; }

	string URI { get; }
	Node Get(string uri);

	void Dump(System.Text.StringBuilder str, int i);
}
public abstract class Node<T>: Node where T:CodeElement {

	public T CodeElement;
	public Node(T ce) {
		CodeElement = ce;
		Children = new List<Node>();
	}

	public Node Parent { get; private set; }
	public IList<Node> Children { get; private set; }

	public void Add<T>(Node<T> node) where T:CodeElement {
		Children.Add(node);
		node.Parent = this;
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
	// Dump() should be private !!
	public void Dump(System.Text.StringBuilder str, int i) {
		for (int c=0; c<i; c++) str.Append("  ");
		if (CodeElement == null) str.AppendLine("?");
		else str.AppendLine(CodeElement.ToString());
		foreach(var child in Children) child.Dump(str, i+1);
	}
}
public class Root: Node<CodeElement> {
	public Root(): base(null) { }
}



/// <summary>A Node who can type its children more strongly should inherit from this.</summary>
/// <typeparam name="C">Class (derived from Node) of the children nodes.</typeparam>
public interface Parent<C> where C:Node { }
/// <summary>Extension method to get children more strongly-typed than just Node.</summary>
public static class ParentExtension {
	public static IList<C> StrongChildren<C>(this Parent<C> parent) where C:Node {
		var node = parent as Node;
		if (node == null) throw new System.NotImplementedException("Parent must be a Node.");
		var result = new List<C>(); //TODO? use ConvertAll or Cast from LINQ
		foreach(var child in node.Children) result.Add((C)child);
        return result;
    }
}
/// <summary>A Node who can type its parent more strongly should inherit from this.</summary>
/// <typeparam name="C">Class (derived from Node) of the parent node.</typeparam>
public interface Child<P> where P:Node { }
/// <summary>Extension method to get a more strongly-typed parent than just Node.</summary>
public static class ChildExtension {
	public static P StrongParent<P>(this Child<P> child) where P:Node {
		var node = child as Node;
		if (node == null) throw new System.NotImplementedException("Child must be a Node.");
		return (P)node.Parent;
    }
}


public class DataDivision: Node<DataDivisionHeader> {
	public DataDivision(DataDivisionHeader header): base(header) { }
}
public class WorkingStorageSection: Node<WorkingStorageSectionHeader> {
	public WorkingStorageSection(WorkingStorageSectionHeader header): base(header) { }
	public override string ID { get { return "working"; } }
}
public class LinkageSection: Node<LinkageSectionHeader> {
	public LinkageSection(LinkageSectionHeader header): base(header) { }
	public override string ID { get { return "linkage"; } }
}
public abstract class DataDefinition<T>: Node<T> where T:DataDefinitionEntry {
	public DataDefinition(T entry): base(entry) { }
	public override string ID { get { return this.CodeElement.Name; } }
	public string Name { get { return CodeElement.Name; } }
}
public class DataDescription: DataDefinition<DataDescriptionEntry>, Parent<DataDescription> {
	public DataDescription(DataDescriptionEntry entry): base(entry) { }
}
public class DataCondition: DataDefinition<DataConditionEntry> {
	public DataCondition(DataConditionEntry entry): base(entry) { }
}
public class TypeDescription: DataDefinition<TypeDescriptionEntry>,
							  Parent<DataDescription>,
							  Child<LinkageSection> // untrue
{
	public TypeDescription(TypeDescriptionEntry entry): base(entry) { }
	public bool IsStrong { get; internal set; }
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
//		program.Enter();
//		program.Exit();
		program.Enter(new DataDivision(new DataDivisionHeader()));
		program.Enter(new WorkingStorageSection(new WorkingStorageSectionHeader()));
		program.Exit();
		program.Enter(new LinkageSection(new LinkageSectionHeader()));
		program.Enter(new DataDescription(new DataDescriptionEntry { Level =  1, Name = "x" }));
		program.Enter(new DataDescription(new DataDescriptionEntry { Level =  5, Name = "a" }));
		program.Enter(new DataDescription(new DataDescriptionEntry { Level = 10, Name = "b" }));
		program.Exit();
		program.Exit();
		program.Enter(new DataDescription(new DataDescriptionEntry { Level = 5, Name = "c" }));
		program.Exit();
		program.Exit();
		program.Enter(new DataDescription(new DataDescriptionEntry { Level = 1, Name = "y" }));
		program.Exit();
		program.Enter(new DataDescription(new DataDescriptionEntry { Level = 1, Name = "z" }));
		program.Exit();
		program.Enter(new DataCondition(new DataConditionEntry { Level = 88, Name = "b" }));
		program.Exit();
		program.Enter(new DataCondition(new DataConditionEntry { Level = 88, Name = "c" }));
		program.Exit();
		program.Enter(new TypeDescription(new TypeDescriptionEntry { Level = 1, Name = "POINT" }));
		program.Enter(new DataDescription(new DataDescriptionEntry { Level =  2, Name = "x" }));
		program.Exit();
		program.Enter(new DataDescription(new DataDescriptionEntry { Level = 2, Name = "y" }));
		program.Exit();
		program.Exit();
		program.Exit();
		program.Exit();

		System.Console.WriteLine(program.Root.ToString());
		System.Console.WriteLine(program.Root.Get("x").ToString());
		System.Console.WriteLine(program.Root.Get("POINT.x").ToString());

		var typedef = program.Root.Get<TypeDescription>("POINT");
System.Console.WriteLine("typedef:"+typedef.URI+"("+typedef.IsStrong+") < "+typedef.StrongParent().URI+" #"+typedef.Children.Count);
		foreach(var child in typedef.StrongChildren()) {
			System.Console.WriteLine("child: "+child.URI+"("+child.Name+")");
		}

		System.Console.ReadLine();
	}
}

