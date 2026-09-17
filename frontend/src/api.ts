import type {
  DailyConsumption,
  Dashboard,
  Device,
  MeterReading,
  Room,
  Tariff,
} from './types'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json', ...(init?.headers ?? {}) },
    ...init,
  })
  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Błąd ${response.status}`)
  }
  if (response.status === 204) {
    return undefined as T
  }
  return (await response.json()) as T
}

export const api = {
  dashboard: () => request<Dashboard>('/api/dashboard'),
  rooms: () => request<Room[]>('/api/rooms'),
  createRoom: (name: string) =>
    request<Room>('/api/rooms', { method: 'POST', body: JSON.stringify({ name }) }),
  deleteRoom: (id: number) => request<void>(`/api/rooms/${id}`, { method: 'DELETE' }),
  devices: () => request<Device[]>('/api/devices'),
  createDevice: (body: unknown) =>
    request<Device>('/api/devices', { method: 'POST', body: JSON.stringify(body) }),
  updateDevice: (id: number, body: unknown) =>
    request<Device>(`/api/devices/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  toggleDevice: (id: number) =>
    request<Device>(`/api/devices/${id}/toggle`, { method: 'PATCH' }),
  deleteDevice: (id: number) => request<void>(`/api/devices/${id}`, { method: 'DELETE' }),
  readings: () => request<MeterReading[]>('/api/readings'),
  createReading: (body: unknown) =>
    request<MeterReading>('/api/readings', { method: 'POST', body: JSON.stringify(body) }),
  deleteReading: (id: number) => request<void>(`/api/readings/${id}`, { method: 'DELETE' }),
  consumption: (from?: string, to?: string) => {
    const query = new URLSearchParams()
    if (from) query.set('from', from)
    if (to) query.set('to', to)
    const qs = query.toString()
    const suffix = qs ? `?${qs}` : ''
    return request<DailyConsumption[]>(`/api/consumption${suffix}`)
  },
  upsertConsumption: (body: unknown) =>
    request<DailyConsumption>('/api/consumption', { method: 'POST', body: JSON.stringify(body) }),
  deleteConsumption: (id: number) =>
    request<void>(`/api/consumption/${id}`, { method: 'DELETE' }),
  tariff: () => request<Tariff>('/api/tariff'),
  saveTariff: (body: unknown) =>
    request<Tariff>('/api/tariff', { method: 'PUT', body: JSON.stringify(body) }),
}
