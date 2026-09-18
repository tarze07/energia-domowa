import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { api } from '../api'
import { fmtDate, fmtKwh, todayIso } from '../format'
import type { DailyConsumption, Tariff } from '../types'

const MONTHS = ['Sty', 'Lut', 'Mar', 'Kwi', 'Maj', 'Cze', 'Lip', 'Sie', 'Wrz', 'Paź', 'Lis', 'Gru']
const WEEKDAYS = ['Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'So', 'Nd']

function isoDay(year: number, monthIndex: number, day: number): string {
  return `${year}-${String(monthIndex + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
}

function heatByLimit(kwh: number, limit: number): string {
  if (kwh > limit) return 'over'
  const ratio = kwh / limit
  if (ratio >= 0.8) return 'l3'
  if (ratio >= 0.5) return 'l2'
  if (ratio > 0) return 'l1'
  return 'l0'
}

function heatByKwh(kwh: number): string {
  if (kwh > 12) return 'over'
  if (kwh > 8) return 'l3'
  if (kwh > 5) return 'l2'
  if (kwh > 0) return 'l1'
  return 'l0'
}

function heatClass(kwh: number | undefined, limit: number): string {
  if (kwh == null) return ''
  if (limit > 0) return heatByLimit(kwh, limit)
  return heatByKwh(kwh)
}

export function HeatmapPage() {
  const year = new Date().getFullYear()
  const today = todayIso()
  const [days, setDays] = useState<DailyConsumption[]>([])
  const [tariff, setTariff] = useState<Tariff | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [selected, setSelected] = useState(today)
  const [kwh, setKwh] = useState('')

  const byDate = useMemo(() => new Map(days.map((d) => [d.date, d])), [days])
  const limit = tariff?.dailyLimitKwh ?? 0
  const picked = byDate.get(selected)

  async function reload() {
    const from = `${year}-01-01`
    const to = `${year}-12-31`
    const [nextDays, nextTariff] = await Promise.all([api.consumption(from, to), api.tariff()])
    setDays(nextDays)
    setTariff(nextTariff)
    return nextDays
  }

  useEffect(() => {
    const from = `${year}-01-01`
    const to = `${year}-12-31`
    Promise.all([api.consumption(from, to), api.tariff()])
      .then(([nextDays, nextTariff]) => {
        setDays(nextDays)
        setTariff(nextTariff)
        const current = nextDays.find((d) => d.date === todayIso())
        setKwh(current ? String(current.kwh) : '')
      })
      .catch((err: unknown) => setError(err instanceof Error ? err.message : 'Błąd'))
  }, [year])

  function pick(date: string) {
    setSelected(date)
    const row = byDate.get(date)
    setKwh(row ? String(row.kwh) : '')
  }

  async function save(event: FormEvent) {
    event.preventDefault()
    setError(null)
    try {
      await api.upsertConsumption({ date: selected, kwh: Number(kwh) })
      await reload()
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać')
    }
  }

  return (
    <div className="stack">
      <header className="page-head">
        <div>
          <p className="eyebrow">Kalendarz</p>
          <h1>Zużycie {year}</h1>
        </div>
      </header>
      {error && <p className="error">{error}</p>}

      <section className="panel">
        <div className="panel-head">
          <h2>Heatmapa roku</h2>
          <small>kliknij dzień, żeby wpisać kWh</small>
        </div>
        <div className="heat-year">
          {MONTHS.map((label, monthIndex) => {
            const count = new Date(year, monthIndex + 1, 0).getDate()
            const pad = (new Date(year, monthIndex, 1).getDay() + 6) % 7
            return (
              <div key={label} className="heat-month">
                <h3>{label}</h3>
                <div className="heat-wd">
                  {WEEKDAYS.map((wd) => (
                    <span key={wd}>{wd}</span>
                  ))}
                </div>
                <div className="heat-days">
                  {Array.from({ length: pad }, (_, i) => (
                    <span key={`pad-${i}`} />
                  ))}
                  {Array.from({ length: count }, (_, i) => {
                    const date = isoDay(year, monthIndex, i + 1)
                    const row = byDate.get(date)
                    const future = date > today
                    const cls = heatClass(row?.kwh, limit)
                    if (future) {
                      return <span key={date} className="heat-cell future" />
                    }
                    return (
                      <button
                        key={date}
                        type="button"
                        className={`heat-cell ${cls}${selected === date ? ' selected' : ''}`}
                        title={`${fmtDate(date)}${row ? ` — ${row.kwh} kWh` : ''}`}
                        onClick={() => pick(date)}
                      />
                    )
                  })}
                </div>
              </div>
            )
          })}
        </div>
        <p className="heat-legend">
          <span>mniej</span>
          <i className="heat-cell l0" />
          <i className="heat-cell l1" />
          <i className="heat-cell l2" />
          <i className="heat-cell l3" />
          <i className="heat-cell over" />
          <span>limit</span>
        </p>
      </section>

      <form className="panel form narrow" onSubmit={(e) => void save(e)}>
        <h2>{fmtDate(selected)}</h2>
        {picked ? <p className="muted">{fmtKwh(picked.kwh)}</p> : <p className="muted">Brak wpisu</p>}
        <label>
          kWh
          <input
            type="number"
            min={0}
            step={0.01}
            value={kwh}
            onChange={(e) => setKwh(e.target.value)}
            required
          />
        </label>
        <button type="submit">Zapisz dzień</button>
      </form>
    </div>
  )
}
