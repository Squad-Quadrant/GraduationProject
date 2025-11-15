using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Presentation.Bootstrap
{
	/// <summary>
	/// A container for managing service lifetimes and dependencies.
	/// </summary>
	public class ServiceContainer
	{
		/// <summary>
		/// metadata about a registered service.
		/// </summary>
		private class ServiceRegistration
		{
			public Type InterfaceType { get; set; }
			public Type ImplementationType { get; set; }
			public EServiceLifetime Lifetime { get; set; }
			public Func<ServiceContainer, object> Factory { get; set; }
		}

		private readonly Dictionary<Type, ServiceRegistration> _registrations = new();
		private readonly Dictionary<Type, object> _instances = new();
		private readonly ServiceContainer _parentContainer;
		private readonly HashSet<Type> _resolving = new(); // To detect circular dependencies

		public ServiceContainer(ServiceContainer parentContainer = null) => _parentContainer = parentContainer;

		#region Registration Methods

		/// <summary>
		/// Registers a service mapping from an interface to an implementation type.
		/// </summary>
		/// <param name="lifetime"></param>
		/// <typeparam name="TInterface"></typeparam>
		/// <typeparam name="TImplementation"></typeparam>
		public void Register<TInterface, TImplementation>(EServiceLifetime lifetime = EServiceLifetime.Singleton)
			where TImplementation : TInterface
		{
			var interfaceType = typeof(TInterface);
			var implementationType = typeof(TImplementation);

			_registrations[interfaceType] = new ServiceRegistration
			{
				InterfaceType = interfaceType,
				ImplementationType = implementationType,
				Lifetime = lifetime,
				Factory = null
			};

			Debug.Log($"[ServiceContainer] Registered {interfaceType.FullName} -> {implementationType.FullName} as {lifetime}");
		}

		/// <summary>
		/// Registers a service using a factory function.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="lifetime"></param>
		/// <typeparam name="TInterface"></typeparam>
		public void Register<TInterface>(Func<ServiceContainer, TInterface> factory,
			EServiceLifetime lifetime = EServiceLifetime.Singleton)
		{
			var interfaceType = typeof(TInterface);

			_registrations[interfaceType] = new ServiceRegistration
			{
				InterfaceType = interfaceType,
				ImplementationType = null,
				Lifetime = lifetime,
				Factory = container => factory(container)
			};

			Debug.Log($"[ServiceContainer] Registered {interfaceType.FullName} via factory as {lifetime}");
		}

		/// <summary>
		/// Directly registers an existing instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <typeparam name="TInterface"></typeparam>
		public void RegisterInstance<TInterface>(TInterface instance)
		{
			var interfaceType = typeof(TInterface);
			_instances[interfaceType] = instance;

			_registrations[interfaceType] = new ServiceRegistration()
			{
				InterfaceType = interfaceType,
				ImplementationType = instance.GetType(),
				Lifetime = EServiceLifetime.Singleton,
				Factory = null
			};

			Debug.Log($"[ServiceContainer] Registered instance of {interfaceType.FullName}");
		}

		#endregion

		#region Resolution Methods

		/// <summary>
		/// Resolves an instance of the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Resolve<T>() => (T)Resolve(typeof(T));

		/// <summary>
		/// Tries to resolve an instance of the specified type, returning null if not registered instead of throwing.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T TryResolve<T>() where T : class
		{
			try
			{
				return Resolve<T>();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Resolves an instance of the specified type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public object Resolve(Type type)
		{
			if (_registrations.TryGetValue(type, out var registration))
				return ResolveFromRegistration(registration);

			if (_parentContainer != null)
				return _parentContainer.Resolve(type);

			throw new InvalidOperationException($"Service of type {type.FullName} is not registered.");
		}

		private object ResolveFromRegistration(ServiceRegistration registration)
		{
			if (registration.Lifetime == EServiceLifetime.Singleton)
			{
				if (_instances.TryGetValue(registration.InterfaceType, out var instance))
					return instance;

				var newInstance = CreateInstance(registration);
				_instances[registration.InterfaceType] = newInstance;
				return newInstance;
			}

			if (registration.Lifetime == EServiceLifetime.Transient)
				return CreateInstance(registration);

			throw new InvalidOperationException($"Unsupported service lifetime: {registration.Lifetime}");
		}

		private object CreateInstance(ServiceRegistration registration)
		{
			// Use reflection to create instance
			var implementationType = registration.ImplementationType;
			if (_resolving.Contains(implementationType))
			{
				throw new InvalidOperationException(
					$"Circular dependency detected while resolving type {implementationType.FullName}");
			}
			_resolving.Add(implementationType);

			try
			{
				// Use factory if available
				if (registration.Factory != null)
					return registration.Factory(this);

				// Use reflection to create instance
				var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
				if (constructors.Length == 0)
					throw new InvalidOperationException(
						$"No public constructors found for type {implementationType.FullName}");

				// Select the constructor with the most parameters
				var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
				var parameters = constructor.GetParameters();
				var parameterInstances = new object[parameters.Length];

				for (int i = 0; i < parameters.Length; i++)
				{
					var parameterType = parameters[i].ParameterType;

					try
					{
						parameterInstances[i] = Resolve(parameterType);
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException(
							$"Failed to resolve parameter '{parameters[i].Name}' of type '{parameterType.Name}' " +
							$"for constructor of '{implementationType.Name}'", ex);
					}
				}

				return constructor.Invoke(parameterInstances);
			}
			finally
			{
				_resolving.Remove(implementationType);
			}
		}

		#endregion

		#region Lifetime Management

		public void Clear()
		{
			foreach (var instance in _instances.Values)
				if (instance is IDisposable disposable)
					disposable.Dispose();
			_instances.Clear();
		}

		public bool IsRegistered<T>() => IsRegistered(typeof(T));

		public bool IsRegistered(Type type)
		{
			if (_registrations.ContainsKey(type))
				return true;
			return _parentContainer?.IsRegistered(type) ?? false;
		}

		#endregion
	}
}
