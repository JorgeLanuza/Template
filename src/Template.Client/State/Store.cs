using System.Collections.Concurrent;

namespace Template.Client.State;

/// <summary>
/// Almacén central de estado (Store) para gestionar la información de la aplicación.
/// </summary>
public class Store
{
	private readonly ConcurrentDictionary<Type, object> _states = new ConcurrentDictionary<Type, object>();

	private readonly ConcurrentDictionary<Type, List<Action<object>>> _listeners = new();

	// Obtiene el estado actual de un tipo específico. Si no existe crea uno nuevo por defecto.
	public TState GetState<TState>()
		where TState : new() => (TState)_states.GetOrAdd(typeof(TState), _ => new TState());

	// Actualiza el estado y notifica a todos los componentes interesados.
	public void SetState<TState>(TState newState)
		where TState : class
	{
		if (newState == null)
			return;

		_states.AddOrUpdate(typeof(TState), newState, (key, oldValue) => newState);
		NotifyListeners(newState);
	}

	// Te suscribe a los cambios de un estado específico.
	public void Subscribe<TState>(Action<TState> callback)
	{
		if (callback == null)
			return;

		_listeners.AddOrUpdate(typeof(TState), [o => callback((TState)o)], (key, list) =>
			{
				list.Add(o => callback((TState)o));

				return list;
			});
	}

	public void Dispatch<TAction>(TAction action) => NotifyListeners(action);

	private void NotifyListeners<T>(T payload)
	{
		if (!_listeners.TryGetValue(typeof(T), out List<Action<object>>? actions))
			return;

		if (actions == null)
			return;

		foreach (Action<object> action in actions)
			action(payload!);
	}
}
