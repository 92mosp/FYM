import React, { useState, useEffect } from 'react';

const API_URL = 'http://localhost:5000/api';

export default function App() {
  const [token, setToken] = useState(localStorage.getItem('token') || '');
  const [username, setUsername] = useState(localStorage.getItem('username') || '');
  const [roles, setRoles] = useState(JSON.parse(localStorage.getItem('roles') || '[]'));
  
  const [loginUser, setLoginUser] = useState('');
  const [loginPass, setLoginPass] = useState('');
  const [error, setError] = useState('');

  const [users, setUsers] = useState([]);
  const [allRoles, setAllRoles] = useState([]);
  
  const [newUsername, setNewUsername] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [newRoleId, setNewRoleId] = useState(1);
  const [successMsg, setSuccessMsg] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const res = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: loginUser, password: loginPass })
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || 'Login failed');
      
      setToken(data.token);
      setUsername(data.username);
      setRoles(data.roles);
      localStorage.setItem('token', data.token);
      localStorage.setItem('username', data.username);
      localStorage.setItem('roles', JSON.stringify(data.roles));
      fetchData(data.token);
    } catch (err) {
      setError(err.message);
    }
  };

  const handleLogout = () => {
    setToken('');
    setUsername('');
    setRoles([]);
    localStorage.clear();
  };

  const fetchData = async (currentToken) => {
    try {
      const uRes = await fetch(`${API_URL}/users`, {
        headers: { 'Authorization': `Bearer ${currentToken}` }
      });
      if (uRes.ok) setUsers(await uRes.json());

      const rRes = await fetch(`${API_URL}/roles`, {
        headers: { 'Authorization': `Bearer ${currentToken}` }
      });
      if (rRes.ok) setAllRoles(await rRes.json());
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    if (token) fetchData(token);
  }, [token]);

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');
    try {
      const res = await fetch(`${API_URL}/users/register`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ username: newUsername, password: newPassword, roleId: Number(newRoleId) })
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || 'Registration failed');
      
      setSuccessMsg('User registered successfully');
      setNewUsername('');
      setNewPassword('');
      fetchData(token);
    } catch (err) {
      setError(err.message);
    }
  };

  if (!token) {
    return (
      <div className="container" style={{maxWidth: '400px'}}>
        <h2>FYM Technology - Login</h2>
        {error && <p className="error">{error}</p>}
        <form onSubmit={handleLogin}>
          <input type="text" placeholder="Username" value={loginUser} onChange={e => setLoginUser(e.target.value)} required />
          <input type="password" placeholder="Password" value={loginPass} onChange={e => setLoginPass(e.target.value)} required />
          <button type="submit">Login</button>
        </form>
      </div>
    );
  }

  const isSuperAdmin = roles.includes('SuperAdmin');

  return (
    <div className="container">
      <h2>Welcome, {username} ({roles.join(', ')})</h2>
      <button onClick={handleLogout} style={{backgroundColor: '#dc3545', width: 'auto', padding: '5px 15px'}}>Logout</button>
      
      <hr style={{margin: '20px 0'}}/>

      {error && <p className="error">{error}</p>}
      {successMsg && <p style={{color: 'green'}}>{successMsg}</p>}

      {isSuperAdmin && (
        <div style={{background: '#f8f9fa', padding: '15px', borderRadius: '5px', marginBottom: '20px'}}>
          <h3>Register New User (SuperAdmin Only)</h3>
          <form onSubmit={handleRegister}>
            <input type="text" placeholder="New Username" value={newUsername} onChange={e => setNewUsername(e.target.value)} required />
            <input type="password" placeholder="New Password" value={newPassword} onChange={e => setNewPassword(e.target.value)} required />
            <select value={newRoleId} onChange={e => setNewRoleId(e.target.value)}>
              {allRoles.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
            </select>
            <button type="submit">Create User</button>
          </form>
        </div>
      )}

      <h3>System Users</h3>
      <table>
        <thead>
          <tr><th>ID</th><th>Username</th><th>Roles</th></tr>
        </thead>
        <tbody>
          {users.map(u => (
            <tr key={u.id}>
              <td>{u.id}</td>
              <td>{u.username}</td>
              <td>{u.id && u.roles ? u.roles.join(', ') : ''}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
