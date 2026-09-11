import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api'
import type { Tariff } from '../types'

export function TariffPage() {
  const [tariff, setTariff] = useState<Tariff | null>(null)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .tariff()
      .then(setTariff)
      .catch((err: unknown) => setError(err instanceof Error ? err.message : 'Błąd'))
  }, [])

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    if (!tariff) {
      return
    }
    setError(null)
    setSaved(false)
    try {
      const next = await api.saveTariff(tariff)
      setTariff(next)
      setSaved(true)
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać')
    }
  }

  if (!tariff) {
    return error ? <p className="error">{error}</p> : <p className="muted">Ładowanie taryfy…</p>
  }

  return (
    <div className="stack">
      <header className="page-head">
        <div>
          <p className="eyebrow">Rozliczenia</p>
          <h1>Taryfa i limity</h1>
        </div>
      </header>
      {error && <p className="error">{error}</p>}
      <form className="panel form narrow" onSubmit={(e) => void onSubmit(e)}>
        <label>
          Nazwa taryfy
          <input value={tariff.name} onChange={(e) => setTariff({ ...tariff, name: e.target.value })} />
        </label>
        <label>
          Cena (zł / kWh)
          <input
            type="number"
            min={0}
            step={0.01}
            value={tariff.pricePerKwh}
            onChange={(e) => setTariff({ ...tariff, pricePerKwh: Number(e.target.value) })}
          />
        </label>
        <label>
          Dzienny limit (kWh)
          <input
            type="number"
            min={0}
            step={0.1}
            value={tariff.dailyLimitKwh}
            onChange={(e) => setTariff({ ...tariff, dailyLimitKwh: Number(e.target.value) })}
          />
        </label>
        <label>
          Budżet miesięczny (zł)
          <input
            type="number"
            min={0}
            step={1}
            value={tariff.monthlyBudgetPln}
            onChange={(e) => setTariff({ ...tariff, monthlyBudgetPln: Number(e.target.value) })}
          />
        </label>
        <button type="submit">Zapisz taryfę</button>
        {saved && <p className="ok">Zapisano.</p>}
      </form>
    </div>
  )
}
