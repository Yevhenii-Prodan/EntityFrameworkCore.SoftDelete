# entity-framework-soft-delete

This code snippet allow to use soft delete only added interface inheritance.

Example: 

Just add an inheritance 
```c#
public class Company : ISoftDeletable
{
    public Guid Id { get; set; }
}
```

And make migration, it will add `DeletedDate` property (`DateTime?`) to your entity.

And that's all you have to do. You just delete entities as usual, and 
instead of physical deleting it will set to `DeletedDate` property current UTC date.

You won't get this entities from your query or includes, but if you want - you can easy
do it:

```c#
// Get with deleted entities
var data = DbContext.TableName.WithDeleted().ToList();

// Get only deleted 
var data = DbContext.TableName.OnlyDeleted().ToList();
```

After you get the deleted entities, you can restore it:

```c#
// restore one entity
var entity =  DbContext.TableName.OnlyDeleted().FirstOrDefault();
DbContext.TableName.Restore(entity);

// resotre range
var entities =  DbContext.TableName.OnlyDeleted().ToList();
DbContext.TableName.Restore(entities);
```


What about relationships?
it also possible, if you add `OnDelete(DeleteBehaviour.SetNull)`
 or `OnDelete(DeleteBehaviour.Cascade)` to your relationship, it will work as usual.
 
 With `Cascade` it will call the `Delete` method on the related entities.
 
 With `SetNull` it will set null in the related entities.
