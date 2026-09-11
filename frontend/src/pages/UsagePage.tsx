import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api'
import { fmtDate, fmtKwh, fmtPln, todayIso } from '../format'
import type { DailyConsumption, MeterReading } from '../types'

export function UsagePage() {
  const [days, setDays] = useState<DailyConsumption[]>([])
  const [readings, setReadings] = useState<MeterReading[]>([])
  const [kwh, setKwh] = useState('8.5')
  const [date, setDate] = useState(todayIso())
  const [meter, setMeter] = useState('')
  const [meterDate, setMeterDate] = useState(todayIso())
  const [note, setNote] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function reload() {
    const [nextDays, nextReadings] = await Promise.all([api.consumption(), api.readings()])
    setDays(nextDays)
    setReadings(nextReadings)
  }

  useEffect(() => {
    reload().catch((err: unknown) => setError(err instanceof Error ? err.message : 'Błąd'))
  }, [])

  async function saveDay(event: FormEvent) {
    event.preventDefault()
    setError(null)
    try {
      await api.upsertConsumption({ date, kwh: Number(kwh) })
      await reload()
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać')
    }
  }

  async function saveReading(event: FormEvent) {
    event.preventDefault()
    setError(null)
    try {
      await api.createReading({ date: meterDate, totalKwh: Number(meter), note: note || null })
      setMeter('')
      setNote('')
      await reload()
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać odczytu')
    }
  }

  return (
    <div className="stack">
      <header className="page-head">
        <div>
          <p className="eyebrow">Pomiary</p>
          <h1>Zużycie i licznik</h1>
        </div>
      </header>
      {error && <p className="error">{error}</p>}

      <section className="grid-2">
        <form className="panel form" onSubmit={(e) => void saveDay(e)}>
          <h2>Zużycie dzienne</h2>
          <label>
            Data
            <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          </label>
          <label>
            kWh
            <input type="number" min={0} step={0.01} value={kwh} onChange={(e) => setKwh(e.target.value)} />
          </label>
          <button type="submit">Zapisz dzień</button>
        </form>
        <form className="panel form" onSubmit={(e) => void saveReading(e)}>
          <h2>Odczyt licznika</h2>
          <label>
            Data
            <input type="date" value={meterDate} onChange={(e) => setMeterDate(e.target.value)} />
          </label>
          <label>
            Stan (kWh)
            <input type="number" min={0} step={0.1} value={meter} onChange={(e) => setMeter(e.target.value)} required />
          </label>
          <label>
            Notatka
            <input value={note} onChange={(e) => setNote(e.target.value)} />
          </label>
          <button type="submit">Dodaj odczyt</button>
        </form>
      </section>

      <section className="grid-2">
        <div className="panel">
          <h2>Historia dni</h2>
          <table>
            <thead>
              <tr>
                <th>Data</th>
                <th>kWh</th>
                <th>Koszt</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {days.slice(0, 16).map((day) => (
                <tr key={day.id}>
                  <td>{fmtDate(day.date)}</td>
                  <td>{fmtKwh(day.kwh)}</td>
                  <td>{fmtPln(day.cost)}</td>
                  <td>
                    <button type="button" className="tiny danger" onClick={() => void api.deleteConsumption(day.id).then(reload)}>
                      Usuń
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="panel">
          <h2>Odczyty</h2>
          <table>
            <thead>
              <tr>
                <th>Data</th>
                <th>Stan</th>
                <th>Δ</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {readings.map((reading) => (
                <tr key={reading.id}>
                  <td>{fmtDate(reading.date)}</td>
                  <td>{fmtKwh(reading.totalKwh)}</td>
                  <td>{reading.deltaKwh == null ? '—' : fmtKwh(reading.deltaKwh)}</td>
                  <td>
                    <button type="button" className="tiny danger" onClick={() => void api.deleteReading(reading.id).then(reload)}>
                      Usuń
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
