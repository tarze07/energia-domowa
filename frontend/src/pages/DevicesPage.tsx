import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api'
import { fmtKwh, fmtPln } from '../format'
import type { Device, Room } from '../types'

const emptyForm = {
  name: '',
  roomId: 0,
  powerWatts: 100,
  hoursPerDay: 1,
  isActive: true,
}

export function DevicesPage() {
  const [rooms, setRooms] = useState<Room[]>([])
  const [devices, setDevices] = useState<Device[]>([])
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [roomName, setRoomName] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function reload() {
    const [nextRooms, nextDevices] = await Promise.all([api.rooms(), api.devices()])
    setRooms(nextRooms)
    setDevices(nextDevices)
    setForm((current) => ({
      ...current,
      roomId: current.roomId || nextRooms[0]?.id || 0,
    }))
  }

  useEffect(() => {
    reload().catch((err: unknown) => setError(err instanceof Error ? err.message : 'Błąd'))
  }, [])

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    const body = {
      name: form.name,
      roomId: form.roomId,
      powerWatts: Number(form.powerWatts),
      hoursPerDay: Number(form.hoursPerDay),
      isActive: form.isActive,
    }
    try {
      if (editingId) {
        await api.updateDevice(editingId, body)
      } else {
        await api.createDevice(body)
      }
      setForm({ ...emptyForm, roomId: rooms[0]?.id ?? 0 })
      setEditingId(null)
      await reload()
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać')
    }
  }

  async function addRoom(event: FormEvent) {
    event.preventDefault()
    if (!roomName.trim()) {
      return
    }
    await api.createRoom(roomName.trim())
    setRoomName('')
    await reload()
  }

  return (
    <div className="stack">
      <header className="page-head">
        <div>
          <p className="eyebrow">Inwentarz</p>
          <h1>Urządzenia i pomieszczenia</h1>
        </div>
      </header>

      {error && <p className="error">{error}</p>}

      <section className="grid-2">
        <form className="panel form" onSubmit={onSubmit}>
          <h2>{editingId ? 'Edycja urządzenia' : 'Nowe urządzenie'}</h2>
          <label>
            Nazwa
            <input
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              required
            />
          </label>
          <label>
            Pomieszczenie
            <select
              value={form.roomId}
              onChange={(e) => setForm({ ...form, roomId: Number(e.target.value) })}
            >
              {rooms.map((room) => (
                <option key={room.id} value={room.id}>
                  {room.name}
                </option>
              ))}
            </select>
          </label>
          <div className="row">
            <label>
              Moc (W)
              <input
                type="number"
                min={1}
                value={form.powerWatts}
                onChange={(e) => setForm({ ...form, powerWatts: Number(e.target.value) })}
              />
            </label>
            <label>
              Godziny / dzień
              <input
                type="number"
                min={0}
                max={24}
                step={0.1}
                value={form.hoursPerDay}
                onChange={(e) => setForm({ ...form, hoursPerDay: Number(e.target.value) })}
              />
            </label>
          </div>
          <label className="check">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
            />
            Aktywne
          </label>
          <div className="row">
            <button type="submit">{editingId ? 'Zapisz' : 'Dodaj'}</button>
            {editingId && (
              <button
                type="button"
                className="ghost"
                onClick={() => {
                  setEditingId(null)
                  setForm({ ...emptyForm, roomId: rooms[0]?.id ?? 0 })
                }}
              >
                Anuluj
              </button>
            )}
          </div>
        </form>

        <form className="panel form" onSubmit={(e) => void addRoom(e)}>
          <h2>Pomieszczenia</h2>
          <label>
            Nowa nazwa
            <input value={roomName} onChange={(e) => setRoomName(e.target.value)} />
          </label>
          <button type="submit">Dodaj pomieszczenie</button>
          <ul className="chips">
            {rooms.map((room) => (
              <li key={room.id}>
                {room.name}
                <span>{room.deviceCount}</span>
                <button
                  type="button"
                  className="tiny"
                  onClick={() => void api.deleteRoom(room.id).then(reload)}
                >
                  ×
                </button>
              </li>
            ))}
          </ul>
        </form>
      </section>

      <section className="panel">
        <table>
          <thead>
            <tr>
              <th>Urządzenie</th>
              <th>Pomieszczenie</th>
              <th>Moc</th>
              <th>h/d</th>
              <th>Dzień</th>
              <th>Miesiąc</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {devices.map((device) => (
              <tr key={device.id} className={device.isActive ? '' : 'dim'}>
                <td>{device.name}</td>
                <td>{device.roomName}</td>
                <td>{device.powerWatts} W</td>
                <td>{device.hoursPerDay}</td>
                <td>{fmtKwh(device.dailyKwh)}</td>
                <td>{fmtPln(device.monthlyCost)}</td>
                <td className="actions">
                  <button type="button" className="tiny" onClick={() => void api.toggleDevice(device.id).then(reload)}>
                    {device.isActive ? 'Wyłącz' : 'Włącz'}
                  </button>
                  <button
                    type="button"
                    className="tiny"
                    onClick={() => {
                      setEditingId(device.id)
                      setForm({
                        name: device.name,
                        roomId: device.roomId,
                        powerWatts: device.powerWatts,
                        hoursPerDay: device.hoursPerDay,
                        isActive: device.isActive,
                      })
                    }}
                  >
                    Edytuj
                  </button>
                  <button type="button" className="tiny danger" onClick={() => void api.deleteDevice(device.id).then(reload)}>
                    Usuń
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}
