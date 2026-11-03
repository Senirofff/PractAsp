import React, { useState } from 'react';
import { createRoot } from 'react-dom/client';
import './style.css';

const api = import.meta.env.VITE_API_URL || 'http://localhost:5057';
const storageKey = 'pharmacy-session';
const corsTest = location.hostname === 'localhost'
  ? 'http://127.0.0.1:5173'
  : 'http://localhost:5173';

function App() {
  const [session, setSession] = useState(() => JSON.parse(sessionStorage.getItem(storageKey) || 'null'));
  const [target, setTarget] = useState(session?.userId ?? '');
  const [register, setRegister] = useState(false);
  const [result, setResult] = useState(null);
  const [busy, setBusy] = useState(false);

  async function request(path, { method = 'GET', body, authenticated = true } = {}) {
    setBusy(true);
    try {
      const headers = {};
      if (body) headers['Content-Type'] = 'application/json';
      if (authenticated && session) headers.Authorization = 'Bearer ' + session.accessToken;
      const response = await fetch(api + path, { method, headers, body: JSON.stringify(body) });
      const data = response.status === 204 ? null : await response.json().catch(() => null);
      const descriptions = {
        401: 'Нужен действительный токен или верные учётные данные.',
        403: 'Роль или владелец профиля не позволяют выполнить запрос.',
        429: 'Лимит входа исчерпан. Повторите через ' + (response.headers.get('Retry-After') ?? 'несколько') + ' с.'
      };
      setResult({
        title: response.status + ' · ' + (descriptions[response.status] ?? (response.ok ? 'Запрос выполнен' : 'Запрос отклонён')),
        data: data?.accessToken ? { userId: data.userId, role: data.role, expiresAt: data.expiresAt } : data
      });
      return response.ok ? data : null;
    } catch {
      setResult({
        title: 'Браузер не предоставил HTTP-ответ',
        data: 'Проверьте доступность API. Если API запущен, откройте Network и Console в DevTools, чтобы проверить, заблокировал ли браузер запрос по CORS.'
      });
      return null;
    } finally {
      setBusy(false);
    }
  }

  async function authenticate(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = await request('/api/auth/' + (register ? 'register' : 'login'), {
      method: 'POST', body: Object.fromEntries(new FormData(form)), authenticated: false
    });
    if (data) {
      sessionStorage.setItem(storageKey, JSON.stringify(data));
      setSession(data);
      setTarget(data.userId);
      form.reset();
    }
  }

  function updateProfile(event) {
    event.preventDefault();
    request('/api/users/' + Number(target) + '/profile', {
      method: 'PATCH', body: Object.fromEntries(new FormData(event.currentTarget))
    });
  }

  return <main>
    <h1>Онлайн-аптека</h1>
    <p>Клиент <code>{location.origin}</code> → API <code>{api}</code></p>
    <section>
      <h2>{register ? 'Регистрация' : 'Вход'}</h2>
      <form onSubmit={authenticate} key={String(register)}>
        <label>Логин<input name="login" autoComplete="username" maxLength={50} required /></label>
        <label>Пароль<input name="password" type="password" autoComplete={register ? 'new-password' : 'current-password'}
          minLength={register ? 8 : undefined} maxLength={128} required /></label>
        {register && <>
          <label>Имя<input name="firstName" maxLength={50} required /></label>
          <label>Фамилия<input name="lastName" maxLength={50} required /></label>
        </>}
        <div className="actions">
          <button disabled={busy}>{register ? 'Создать профиль' : 'Войти'}</button>
          <button type="button" onClick={() => setRegister(!register)}>
            {register ? 'Ко входу' : 'Зарегистрироваться'}
          </button>
        </div>
      </form>
      {session && <p>Сессия #{session.userId} · {session.role} <button onClick={() => {
        sessionStorage.removeItem(storageKey);
        setSession(null);
      }}>Выйти</button></p>}
    </section>
    <section>
      <h2>Защищённые запросы</h2>
      <div className="actions">
        <button disabled={busy || !session} onClick={() => request('/api/users/' + session.userId)}>Мой профиль</button>
        <button disabled={busy} onClick={() => request('/api/users')}>Список пользователей (Admin)</button>
        <button disabled={busy} onClick={() => request('/api/users', { authenticated: false })}>Запрос без токена</button>
      </div>
      <form onSubmit={updateProfile}>
        <label>ID профиля<input type="number" min="1" value={target} onChange={event => setTarget(event.target.value)} required /></label>
        <label>Новое имя<input name="firstName" maxLength={50} required /></label>
        <label>Новая фамилия<input name="lastName" maxLength={50} required /></label>
        <div className="actions">
          <button type="button" disabled={busy || !target} onClick={() => request('/api/users/' + Number(target))}>Открыть по ID</button>
          <button disabled={busy || !session}>Изменить профиль</button>
        </div>
      </form>
    </section>
    <section aria-live="polite">
      <h2>Ответ API</h2>
      <p>{busy ? 'Выполняется запрос…' : result?.title ?? 'Выберите действие.'}</p>
      {result?.data && <pre>{JSON.stringify(result.data, null, 2)}</pre>}
      <a href={corsTest} target="_blank" rel="noreferrer">Открыть клиент через другой origin</a>
    </section>
  </main>;
}

createRoot(document.getElementById('root')).render(<App />);
